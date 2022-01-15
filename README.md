# regression-state-machine
Regression Test State Machine

## Overview
This project uses reflrection given a state class to find all viable paths statically and then runs them as a sequence. There is also a runtime validator to make sure promised return value of function declared via an attribute statically has been followed. It uses modified version of topological sort to find all possible topological sorts of a graph. Additionally, it uses concept of negative edges to avoid going from one state to another if there is no viable path.

## Usage
- "State" is a method that does not return `void` (or `Task` with no generic parameter) and declared with `[State]`
- `[Declaration]` describes the promised result of "state"
- `[BoundValue]` maps statically bounded parameter to be looked up from a dictionary provided at runtime
- `[Guard]` describes the acceptable properties of parameter candidate

```csharp
// This class defines the aspects of the finite state machine
public class TestAspects
{
    [State]
    // This state (or method) promises to produce a type of A with property "Name" equal to "foo"
    [Declaration(typeof(A), "Name", SubsetExpressionType.Equal, "foo")]
    public A Step1(
        // This parameter is bounded to a variable "key"
        [BoundValue("key")] string dummy)
    {
        return new A
        {
            Name = "foo"
        };
    }
    
    [State]
    // This state (or method) promises to produce a type of B with property "Name" equal to "bar"
    [Declaration(typeof(B), "Name", SubsetExpressionType.Equal, "bar")]
    public async Task<B> Step2(
        // This parameter not only should be of type of A but it also
        // should have a proprty "Name" with value of "foo"
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
    // This state (or method) promises to produce a type of C with property "Name" equal to "baz"
    [Declaration(typeof(C), "Name", SubsetExpressionType.Equal, "baz")]
    public C Step3(
        // This parameter not only should be of type of B but it also
        // should have a proprty "Name" with value of "bar"
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
