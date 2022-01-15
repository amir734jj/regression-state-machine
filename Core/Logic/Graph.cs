using System;
using System.Collections.Generic;
using System.Linq;
using Core.Extensions;

namespace Core.Logic
{
    public class Graph<T>
    {
        private readonly HashSet<T> _vertices;

        //  No. of vertices
        private readonly Dictionary<T, List<T>> _edgeSet;

        private readonly Dictionary<T, List<T>> _negativeEdgeSet;

        public Graph(HashSet<T> vertices)
        {
            _vertices = vertices;
            _edgeSet = vertices.ToDictionary(x => x, _ => new List<T>());
            _negativeEdgeSet = vertices.ToDictionary(x => x, _ => new List<T>());
        }

        //  Utility function to add edge
        public void AddEdge(T src, T dest)
        {
            _edgeSet[src].Add(dest);
        }

        public void AddNegativeEdge(T src, T dest)
        {
            _negativeEdgeSet[src].Add(dest);
        }

        //  Main recursive function to print all possible
        //  topological sorts
        private IEnumerable<List<T>> AllTopologicalSortsUtil(
            Dictionary<T, bool> visited,
            Dictionary<T, int> inDegree,
            Stack<T> stack)
        {
            if (inDegree == null) throw new ArgumentNullException(nameof(inDegree));

            //  To indicate whether all topological are found or not
            var flag = false;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var node in _vertices)
            {
                // ReSharper disable once InvertIf
                if (!visited.GetValueOrDefault(node, false) && inDegree.GetValueOrDefault(node, 0) == 0)
                {
                    // including in result
                    visited[node] = true;
                    stack.Push(node);
                    foreach (var adjacent in _edgeSet.GetValueOrDefault(node, new List<T>())!)
                    {
                        inDegree[adjacent]--;
                    }

                    foreach (var list in AllTopologicalSortsUtil(visited, inDegree, stack))
                    {
                        yield return list;
                    }

                    // resetting visited, res and in degree for backtracking
                    visited[node] = false;
                    stack.Pop();

                    foreach (var adjacent in _edgeSet.GetValueOrDefault(node, new List<T>())!)
                    {
                        inDegree[adjacent]++;
                    }

                    flag = true;
                }
            }

            //  We reach here if all vertices are visited.
            // ReSharper disable once InvertIf
            if (!flag && stack.Any())
            {
                var sList = stack.ToList();
                // Need to reverse the stack to get correct enumeration
                sList.Reverse();

                var result = new List<T>();

                for (var i = 1; i < sList.Count; i++)
                {
                    var source = sList[i - 1];
                    var destination = sList[i];
                    if (!_negativeEdgeSet[source].Contains(destination) && _edgeSet[source].Contains(destination))
                    {
                        result.Add(source);
                    }

                    if (i + 1 == sList.Count)
                    {
                        result.Add(destination);
                    }
                }

                if (result.Count > 0)
                {
                    yield return result;
                }
            }
        }

        //  The function does all Topological Sort.
        //  It uses recursive all topologicalSortUtil()
        public HashSet<List<T>> AllTopologicalSorts()
        {
            //  Mark all the vertices as not visited
            var visited = new Dictionary<T, bool>();
            var inDegree = new Dictionary<T, int>();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var (_, neighbors) in _edgeSet)
            {
                foreach (var neighbor in neighbors)
                {
                    inDegree[neighbor] = inDegree.GetValueOrDefault(neighbor, 0) + 1;
                }
            }

            var stack = new Stack<T>();
            var result = AllTopologicalSortsUtil(visited, inDegree, stack).ToList();

            return result.DistinctBy(x => string.Join(',', x.Select(y => y.ToString()))).ToHashSet();
        }
    }
}