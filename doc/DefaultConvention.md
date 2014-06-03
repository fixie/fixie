## Default Convention

When using the default convention, a test class is any concrete class in your test assembly whose name ends with "Tests".  Within such a test class, a test case is any public instance void method.  Additionally, test cases include public instance async methods returning `Task` or `Task<T>`.

One instance of your test class is constructed for *each* test case. To perform setup steps before each test case executes, use the test class's default constructor. To perform cleanup steps after each test cases executes, implement `IDisposable` and place cleanup code within the `Dispose()` method.

No [Attributes], no "using Fixie;" statement, no muss, no fuss.

```cs
using Should;

public class CalculatorTests
{
    readonly Calculator calculator;

    public CalculatorTests()
    {
        calculator = new Calculator();
    }

    public void ShouldAdd()
    {
        calculator.Add(2, 3).ShouldEqual(5);
    }

    public void ShouldSubtract()
    {
        calculator.Subtract(5, 3).ShouldEqual(2);
    }

    public async Task SupportsAsyncTestCases()
    {
        int result = await AddAsync(2, 3);

        result.ShouldEqual(5);
    }

    private Task<int> AddAsync(int x, int y)
    {
        return Task.Run(() => calculator.Add(x, y));
    }
}
```