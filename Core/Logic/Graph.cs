using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Logic
{
    public class Graph<T>
    {
        private readonly HashSet<T> _vertices;

        //  No. of vertices
        private readonly Dictionary<T, List<T>> _adjListArray;

        public Graph(HashSet<T> vertices)
        {
            _vertices = vertices;
            _adjListArray = vertices.ToDictionary(x => x, x => new List<T>());
        }

        //  Utility function to add edge
        public void AddEdge(T src, T dest)
        {
            if (!_adjListArray.ContainsKey(src))
            {
                _adjListArray[src] = new List<T>();
            }

            _adjListArray[src].Add(dest);
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
                    foreach (var adjacent in _adjListArray.GetValueOrDefault(node, new List<T>())!)
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

                    foreach (var adjacent in _adjListArray.GetValueOrDefault(node, new List<T>())!)
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
                
                // Add everything else
                var result = sList.Concat(_vertices.Where(v => !sList.Contains(v))).ToList();

                // Ensure connectedness
                if (result.Skip(1).Zip(Enumerable.Range(1, result.Count - 1))
                        .All(x => _adjListArray[result[x.Second - 1]].Contains(x.First)))
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
            foreach (var (_, neighbors) in _adjListArray)
            {
                foreach (var neighbor in neighbors)
                {
                    inDegree[neighbor] = inDegree.GetValueOrDefault(neighbor, 0) + 1;
                }
            }

            var stack = new Stack<T>();
            
            return AllTopologicalSortsUtil(visited, inDegree, stack).ToHashSet();
        }
    }
}