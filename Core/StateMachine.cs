using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Attributes;
using Core.Exceptions;
using Core.Extensions;
using Core.Interfaces;
using Core.Logic;
using Core.Models;
using Microsoft.Extensions.Logging;
using static System.String;

namespace Core
{
    public class StateMachine<T> : IStateMachine<T>
    {
        private readonly ILogger<StateMachine<T>> _logger;
        
        private readonly HashSet<List<State>> _recipes;

        public StateMachine(ILogger<StateMachine<T>> logger)
        {
            _logger = logger;
            
            var states = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<StateAttribute>() != null)
                .Select(state =>
                {
                    if (state.ReturnType == typeof(void))
                    {
                        logger.LogError("{} returns void as result, it should return a (async) type", state);
                        
                        throw new ArgumentException("State should not return void result.");
                    }

                    var parameters = state.GetParameters().ToList();
                    var isAsync = state.ReturnType.IsGenericType &&
                                  state.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

                    if (isAsync && parameters.Count == 0)
                    {
                        logger.LogError("{} returns void async result, it should return a (async) type", state);
                        
                        throw new ArgumentException("Async state should not return void result.");
                    }

                    var returnType = isAsync ? state.ReturnType.GetGenericArguments()[0] : state.ReturnType;
                    var declAttrs = state.GetCustomAttributes<DeclarationAttribute>().ToList();
                    var paramAttrs = parameters
                        .Where(y => y.GetCustomAttribute<BoundValueAttribute>() == null)
                        .ToDictionary(y => y, y => y.GetCustomAttributes<GuardAttribute>().ToList());
                    var boundAttrs = parameters
                        .Where(y => y.GetCustomAttribute<BoundValueAttribute>() != null)
                        .ToDictionary(y => y, y => y.GetCustomAttribute<BoundValueAttribute>());

                    if (paramAttrs.Any(y => y.Value.Any() && y.Value.All(z => z.Type != y.Key.ParameterType)))
                    {
                        logger.LogError("{} has guard over parameters that are inconsistently typed", state);
                        
                        throw new ArgumentException("Inconsistent guard type over parameter.");
                    }

                    if (declAttrs.Any(y => y.Type != returnType))
                    {
                        logger.LogError("{} has declaration over method that are inconsistently typed", state);
                        
                        throw new ArgumentException("Inconsistent declaration type over return type.");
                    }

                    if (paramAttrs.Any(y =>
                            y.Value.Combinations(paramAttrs[y.Key])
                                .Where(z => !z.Item1.Equals(z.Item2))
                                .Any(z =>
                                    z.Item1.IsSubsetOf(z.Item2) || z.Item2.IsSubsetOf(z.Item1))))
                    {
                        logger.LogError("{} overlap between parameter guards that prevents static scheduling", state);
                        
                        throw new ArgumentException("Overlap between guards is not allowed.");
                    }

                    if (declAttrs.Combinations(declAttrs)
                        .Where(y => !y.Item1.Equals(y.Item2))
                        .Any(y => y.Item1.IsSubsetOf(y.Item2) || y.Item2.IsSubsetOf(y.Item1)))
                    {
                        logger.LogError("{} overlap between declarations that prevents static scheduling", state);
                        
                        throw new ArgumentException("Overlap between declarations is not allowed.");
                    }

                    return new State
                    {
                        Name = state.Name,
                        MethodInfo = state,
                        Parameters = parameters,
                        IsAsync = isAsync,
                        ReturnType = returnType,
                        Declarations = declAttrs,
                        ParameterGuards = paramAttrs,
                        BoundParameters = boundAttrs,
                    };
                })
                .ToList();

            if (states.Any(state =>
                    state.ParameterGuards.Keys.Any(p =>
                        states.Except(new[] { state }).All(x => x.ReturnType != p.ParameterType))))
            {
                logger.LogError("Cannot find a valid that supplies parameter for a state {}", states);

                throw new ArgumentException("Cannot find a valid state that supplies parameter type for a state.");
            }
            
            if (states.Any(state =>
                    state.ParameterGuards
                        .SelectMany(x => x.Value)
                        .Any(p => !states.Except(new[] { state })
                            .SelectMany(x => x.Declarations)
                            .Where(x => x.IsRelatedTo(p))
                            .Any(x => x.IsSubsetOf(p)))))
            {
                logger.LogError("Cannot find a valid that supplies parameter for a state {}", states);
                
                throw new ArgumentException("Cannot find a valid state that supplies parameter for a state.");
            }

            var graphRecipeBuilder = new RecipeBuilder(states);

            _recipes = graphRecipeBuilder.Recipes;

            _logger.LogInformation("Found these recipes: {}", Join(';', _recipes.Select(x => Join(',', x))));
        }

        public async Task Run(Dictionary<string, object> dict, T instance)
        {
            if (_recipes.SelectMany(x => x).SelectMany(x => x.BoundParameters.Values).Select(x => x.Name)
                    .Except(dict.Keys).Count() != 0)
            {
                throw new ArgumentException("Dictionary of bounded parameters is not complete.", nameof(dict));
            }

            foreach (var sort in _recipes)
            {
                _logger.LogInformation("Starting a sequence: {}", Join(',', sort));
                
                var dynamicBag = new LinkedList<(Type type, object result)>();
                foreach (var state in sort)
                {
                    _logger.LogInformation("Starting: {}", state);
                    var parameters = state.Parameters.Select(p =>
                    {
                        if (state.BoundParameters.ContainsKey(p))
                        {
                            return dict[state.BoundParameters[p].Name];
                        }

                        // ReSharper disable once InvertIf
                        if (dynamicBag.Any(x => x.Item1 == p.ParameterType))
                        {
                            foreach (var (_, dynamicVal) in dynamicBag.Where(x => x.Item1 == p.ParameterType))
                            {
                                if (state.ParameterGuards.ContainsKey(p) &&
                                    state.ParameterGuards[p].All(x => x.Validator(dynamicVal)))
                                {
                                    return dynamicVal;
                                }
                            }
                        }

                        _logger.LogError("Cannot supply parameter: {} in state: {}", p.Name, state);
                        
                        throw new RuntimeException($"Cannot supply parameter: {p.Name} in state: {state}.");
                    });

                    object result;
                    if (state.IsAsync)
                    {
                        var asyncResult = (Task)state.MethodInfo.Invoke(instance, parameters.ToArray());
                        await asyncResult!.ConfigureAwait(false);
                        result = asyncResult.GetType().GetProperty("Result")?.GetValue(asyncResult);
                    }
                    else
                    {
                        result = state.MethodInfo.Invoke(instance, parameters.ToArray());
                    }

                    if (state.Declarations.Any(declaration => !declaration.Validator(result)))
                    {
                        _logger.LogError("{} promised declaration has not been followed", state);
                        
                        throw new RuntimeException($"{state} promised declaration has not been followed.");
                    }

                    dynamicBag.AddFirst((state.ReturnType, result));

                    _logger.LogInformation("Finished: {}", state);
                }
            }
        }
    }
}