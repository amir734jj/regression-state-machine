using System.Linq;
using Core.Models;

namespace Core.Logic
{
    internal class StateAnalyzer
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public bool CanBeConnected(State source, State destination)
        {
            foreach (var (_, guards) in destination.ParameterGuards
                         .Where(p => p.Key.ParameterType == source.ReturnType))
            {
                if (guards!.All(guard =>
                        source.Declarations
                            .Where(decl => decl.IsRelatedTo(guard))
                            .All(decl => decl.IsSubsetOf(guard))))
                {
                    return true;
                }
            }

            return false;
        }
    }
}