using System.Collections.Generic;
using System.Linq;
using Core.Attributes;
using Core.Models;

namespace Core.Logic
{
    public class StateAnalyzer
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public bool CanBeConnected(State source, State destination)
        {
            foreach (var parameterInfo in destination.Parameters
                         .Where(p => !destination.BoundParameters.ContainsKey(p) && p.ParameterType == source.ReturnType))
            {
                var guards = destination.ParameterGuards.GetValueOrDefault(parameterInfo, new List<GuardAttribute>());
                if (guards!.All(guard =>
                        source.Declarations.All(decl => decl.IsSubsetOf(guard))))
                {
                    return true;
                }

                return false;
            }

            return false;
        }
    }
}