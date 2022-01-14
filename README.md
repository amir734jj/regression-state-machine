# regression-state-machine
Regression Test State Machine

Notes:
- "State" is a method that does not return `void` (or `Task` with no generic parameter) and declared with `[State]`
- `[Declaration]` describes the promised result of "state"
- `[BoundValue]` maps statically bounded parameter to be looked up from a dictionary provided at runtime
- `[Guard]` describes the acceptable properties of parameter candidate

Code finds all viable paths statically and then runs them as a sequence.

```csharp
public class TestAspects
{
    [State]
    [Declaration(typeof(A), "Name", SubsetExpressionType.Equal, "foo")]
    public A Step1([BoundValue("key")] string dummy)
    {
        return new A
        {
            Name = "foo"
        };
    }
    
    [State]
    [Declaration(typeof(B), "Name", SubsetExpressionType.Equal, "bar")]
    public async Task<B> Step2(
        [Guard(typeof(A), "Name", SubsetExpressionType.Equal, "foo")]
        A a)
    {
        await Task.Delay(2000);
        
        return new B
        {
            Name = "bar"
        };
    } 
    
    [State]
    [Declaration(typeof(C), "Name", SubsetExpressionType.Equal, "baz")]
    public C Step3(
        [Guard(typeof(B), "Name", SubsetExpressionType.Equal, "bar")]
        B b)
    {
        return new C
        {
            Name = "baz"
        };
    } 
}
```
