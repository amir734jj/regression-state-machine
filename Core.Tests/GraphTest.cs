using System.Collections.Generic;
using System.Linq;
using Core.Logic;
using Xunit;

namespace Core.Tests
{
    public class GraphTest
    {
        [Fact]
        public void Test_AllTopologicalSorts()
        {
            var graph = new Graph<int>(Enumerable.Range(0, 6).ToHashSet());
            graph.AddEdge(5, 2);
            graph.AddEdge(5, 0);
            graph.AddEdge(4, 0);
            graph.AddEdge(4, 1);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 1);

            var expected = new HashSet<List<int>>
            {
                new List<int> { 4, 5, 0, 2, 3, 1 },
                new List<int> { 4, 5, 2, 0, 3, 1 },
                new List<int> { 4, 5, 2, 3, 0, 1 },
                new List<int> { 4, 5, 2, 3, 1, 0 },
                new List<int> { 5, 2, 3, 4, 0, 1 },
                new List<int> { 5, 2, 3, 4, 1, 0 },
                new List<int> { 5, 2, 4, 0, 3, 1 },
                new List<int> { 5, 2, 4, 3, 0, 1 },
                new List<int> { 5, 2, 4, 3, 1, 0 },
                new List<int> { 5, 4, 0, 2, 3, 1 },
                new List<int> { 5, 4, 2, 0, 3, 1 },
                new List<int> { 5, 4, 2, 3, 0, 1 },
                new List<int> { 5, 4, 2, 3, 1, 0 }
            };

            Assert.Equal(expected.Count, graph.AllTopologicalSorts().Count);
            
            foreach (var sort in graph.AllTopologicalSorts())
            {
                Assert.Contains(expected, x => x.SequenceEqual(sort));
            }
        }
    }
}