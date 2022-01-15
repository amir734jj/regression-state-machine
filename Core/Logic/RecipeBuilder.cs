using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models;

namespace Core.Logic
{
    internal class RecipeBuilder
    {
        public RecipeBuilder(List<State> states)
        {
            var graph = new Graph<State>(states.ToHashSet());
            var stateAnalyzer = new StateAnalyzer();
            
            foreach (var source in states)
            {
                foreach (var destination in states
                             .Where(destination => source != destination))
                {
                    if (stateAnalyzer.CanBeConnected(source, destination))
                    {
                        graph.AddEdge(source, destination);
                    }
                    else
                    {
                        graph.AddNegativeEdge(source, destination);
                    }
                }
            }

            Recipes = graph.AllTopologicalSorts();

            if (Recipes.Count == 0)
            {
                throw new ArgumentException("Failed to find sound any recipe.", nameof(states));
            }
        }

        public HashSet<List<State>> Recipes { get; set; }
    }
}