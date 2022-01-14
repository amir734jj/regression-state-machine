using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Core.Attributes;
using Core.Models;

namespace ConsoleApp
{
    public class A
    {
        public string Name { get; set; }
    }

    public class B
    {
        public string Name { get; set; }
    }

    public class C
    {
        public string Name { get; set; }
    }
    
    public class TestAspects
    {
        [State]
        [Declaration(typeof(A), "Name", SubsetExpressionType.Equal, "amir")]
        public A Step1([BoundValue("foo")]string amir)
        {
            return new A
            {
                Name = "amir"
            };
        }
        
        [State]
        [Declaration(typeof(B), "Name", SubsetExpressionType.Equal, "taha")]
        public async Task<B> Step2(
            [Guard(typeof(A), "Name", SubsetExpressionType.Equal, "amir")]
            A a)
        {
            await Task.Delay(2000);
            
            return new B
            {
                Name = "taha"
            };
        } 
        
        [State]
        [Declaration(typeof(C), "Name", SubsetExpressionType.Equal, "zack")]
        public C Step3(
            [Guard(typeof(B), "Name", SubsetExpressionType.Equal, "taha")]
            B b)
        {
            return new C
            {
                Name = "zack"
            };
        } 
    }
    
    class Program
    {
        static async Task Main(string[] args)
        {
            var dict = new Dictionary<string, object>
            {
                ["foo"] = "bar"
            };
            
            var fsm = new StateMachine<TestAspects>();
            await fsm.Run(dict, new TestAspects());
        }
    }
}