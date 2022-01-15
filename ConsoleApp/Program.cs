using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Core.Attributes;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}

namespace ConsoleApp
{
    public record A(int Val);
    public record B(int Val);
    public record C(int Val);
    
    public class TestAspects
    {
        [State]
        [Declaration(typeof(A), "Val", SubsetExpressionType.Equal, 1)]
        public A ProduceA1([BoundValue("key")] string _) => new A(1);
        
        [State]
        [Declaration(typeof(A), "Val", SubsetExpressionType.Equal, 2)]
        public A ProduceA2([BoundValue("key")] string _) => new A(2);
        
        [State]
        [Declaration(typeof(A), "Val", SubsetExpressionType.Equal, 1)]
        public A ProduceA1Alt([BoundValue("key")] string _) => new A(1);
        
        [State]
        [Declaration(typeof(A), "Val", SubsetExpressionType.Equal, 2)]
        public A ProduceA2Alt([BoundValue("key")] string _) => new A(2);

        [State]
        [Declaration(typeof(B), "Val", SubsetExpressionType.Equal, 3)]
        public async Task<B> ProduceB(
            [BoundValue("key")] string _,
            [Guard(typeof(A), "Val", SubsetExpressionType.Equal, 1)] A x,
            [Guard(typeof(A), "Val", SubsetExpressionType.Equal, 2)] A y) => new B(3);

        [State]
        [Declaration(typeof(C), "Val", SubsetExpressionType.Equal, 4)]
        public C ProduceC(
            [Guard(typeof(B), "Val", SubsetExpressionType.NotEqual, -1)] B b) => new C(4);
        
        [State]
        [Declaration(typeof(C), "Val", SubsetExpressionType.Equal, 5)]
        public C ProduceCAlt(
            [Guard(typeof(B), "Val", SubsetExpressionType.NotEqual, -1)] B bx,
            [Guard(typeof(B), "Val", SubsetExpressionType.NotEqual, -1)] B by) => new C(5);
    }
    
    class Program
    {
        static async Task Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(x => x.AddConsole());
            
            var dict = new Dictionary<string, object>
            {
                ["key"] = "Hello world!"
            };

            var fsm = new StateMachine<TestAspects>(loggerFactory.CreateLogger<StateMachine<TestAspects>>());
            
            await fsm.Run(dict, new TestAspects());
        }
    }
}