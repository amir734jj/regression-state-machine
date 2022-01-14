using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Attributes;
using Core.Exceptions;
using Core.Logic;
using Core.Models;

namespace Core
{
    public class StateMachine<T>
    {
        private readonly HashSet<List<State>> _recipes;

        public StateMachine()
        {
            var states = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<StateAttribute>() != null)
                .Select(x =>
                {
                    if (x.ReturnType == typeof(void))
                    {
                        throw new ArgumentException("State should not return void result.");
                    }

                    var parameters = x.GetParameters().ToList();
                    var isAsync = x.ReturnType.IsGenericType &&
                                  x.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

                    if (isAsync && parameters.Count == 0)
                    {
                        throw new ArgumentException("Async state should not return void result.");
                    }

                    var returnType = isAsync ? x.ReturnType.GetGenericArguments()[0] : x.ReturnType;
                    var declAttrs = x.GetCustomAttributes<DeclarationAttribute>().ToList();
                    var paramAttrs = parameters
                        .Where(y => y.GetCustomAttribute<BoundValueAttribute>() == null)
                        .ToDictionary(y => y, y => y.GetCustomAttributes<GuardAttribute>().ToList());
                    var boundAttrs = parameters
                        .Where(y => y.GetCustomAttribute<BoundValueAttribute>() != null)
                        .ToDictionary(y => y, y => y.GetCustomAttribute<BoundValueAttribute>());

                    if (paramAttrs.Any(y => y.Value.Any() && y.Value.All(z => z.Type != y.Key.ParameterType)))
                    {
                        throw new ArgumentException("Inconsistent guard type over parameter.");
                    }

                    return new State
                    {
                        Name = x.Name,
                        MethodInfo = x,
                        Parameters = parameters,
                        IsAsync = isAsync,
                        ReturnType = returnType,
                        Declarations = declAttrs,
                        ParameterGuards = paramAttrs,
                        BoundParameters = boundAttrs,
                    };
                })
                .ToList();
            
            if (states.Any(state => state.ParameterGuards.Keys.Any(p => states.Except(new[] { state }).All(x => x.ReturnType != p.ParameterType))))
            {
                throw new ArgumentException("Cannot find a valid state that supplies parameter for a state.");
            }

            var graphRecipeBuilder = new RecipeBuilder(states);

            _recipes = graphRecipeBuilder.Recipes;
        }

        public async Task Run(Dictionary<string, object> dict, T instance)
        {
            if (_recipes.SelectMany(x => x).SelectMany(x => x.BoundParameters.Values).Select(x => x.Name).Except(dict.Keys).Count() != 0)
            {
                throw new ArgumentException("Dictionary of bounded parameters is not complete.", nameof(dict));
            }
            
            foreach (var sort in _recipes)
            {
                Console.WriteLine($"Starting a sequence: {string.Join(',', sort)}.");
                var dynamicBag = new LinkedList<(Type type, object result)>();
                foreach (var state in sort)
                {
                    Console.WriteLine($"Starting: {state}");
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
                                if (state.ParameterGuards.ContainsKey(p) && state.ParameterGuards[p].All(x => x.Validator(dynamicVal)))
                                {
                                    return dynamicVal;
                                }
                            }
                        }
                        
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
                        throw new RuntimeException($"{state} promised declaration has not been followed.");
                    }

                    dynamicBag.AddFirst((state.ReturnType, result));

                    Console.WriteLine($"Finished: {state}.");
                }
            }
        }
    }
}