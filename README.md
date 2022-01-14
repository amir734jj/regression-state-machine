# regression-state-machine
Regression Test State Machine

Notes:
- "State" is a method that does not return `void` (or `Task` with no generic parameter) and declared with `[State]`
- `[Declaration]` describes the promised result of "state"
- `[BoundValue]` maps statically bounded parameter to be looked up from a dictionary provided at runtime
- `[Guard]` describes the acceptable properties of parameter candidate

```csharp
public class TestAspects
{
    [State]
    [Declaration(typeof(A), "Name", SubsetExpressionType.Equal, "amir")]
    public A Step1([BoundValue("foo")] string dummy)
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
```
