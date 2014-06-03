## How do I make assertions?

Most test frameworks such as NUnit or xUnit include their own assertion libraries so that you can make statements like this:

```cs
Assert.AreEqual(expected, actual);
```

Assertion libraries are orthogonal to test frameworks.  Your choice of assertion library should be independent of your choice of test framework.  Therefore, Fixie will *never* include an assertion library.

Here are some useful third-party assertion libraries:

* [Should](https://nuget.org/packages/Should/)
* [Shouldly](https://nuget.org/packages/Shouldly/)
* [Fluent Assertions](https://www.nuget.org/packages/FluentAssertions)

## How do I hide assertion library implementation details?

NUnit, for instance, simplifies its own output when exceptions are thrown by its own assertion library infrastructure.  For instance, when NUnit's `Assert.AreEqual(int, int)` fails by throwing an exception, the output deliberately excludes stack trace lines *within* the implementation of `AreEqual`, and deliberately excludes the name of the exception type.  This filtering allows typical test failure output to remain as simple and direct as possible, pointing the developer to the line where their own test failed.

Since Fixie has no assertion library of its own, you may instruct it which types make up your assertion library's implementation details.  Consider some hypothetical assertion library:

```cs
namespace Hypothetical.Assertion.Library
{
    public static class Assert
    {
        public static void AreEqual(int expected, int actual)
        {
            if (expected != actual)
                throw new AssertionException(String.Format("Expected {0} but was {1}.", expected, actual));
        }
    }

    public class AssertionException : Exception
    {
        public AssertionException(string message)
            : base(message)
        {
        }
    }
}
```

Out of the box, Fixie doesn't distinguish AssertionException from any other Exception, so all of the exception details appear in the output.  Consider a test class with some tests that will surely fail and the corresponding default verbose output:

```cs
using Hypothetical.Assertion.Library;

public class CalculatorTests
{
    readonly Calculator calculator;

    public CalculatorTests()
    {
        calculator = new Calculator();
    }

    public void ShouldAdd()
    {
        Assert.AreEqual(999, calculator.Add(2, 3));
    }

    public void ShouldSubtract()
    {
        Assert.AreEqual(999, calculator.Subtract(5, 3));
    }
}
```

```
Test 'CalculatorTests.ShouldAdd' failed: Hypothetical.Assertion.Library.AssertionException
    Expected 999 but was 5.
    Conventions\DefaultConvention.cs(10,0): at Hypothetical.Assertion.Library.Assert.AreEqual(Int32 expected, Int32 actual)
    CalculatorTests.cs(14,0): at CalculatorTests.ShouldAdd()

Test 'CalculatorTests.ShouldSubtract' failed: Hypothetical.Assertion.Library.AssertionException
    Expected 999 but was 2.
    Conventions\DefaultConvention.cs(10,0): at Hypothetical.Assertion.Library.Assert.AreEqual(Int32 expected, Int32 actual)
    CalculatorTests.cs(19,0): at CalculatorTests.ShouldSubtract()
```

The implementation details of assertion libraries are rarely interesting to the developer.  A custom convention can be instructed to simplify failure output by listing the types that make up the assertion library:

```cs
public class CustomConvention : Convention
{
    public CustomConvention()
    {
        Classes
            .NameEndsWith("Tests");

        Methods
            .Where(method => method.IsVoid());

        HideExceptionDetails
            .For<AssertionException>()
            .For(typeof(Assert));
    }
}
```

Rerunning the failing tests, Fixie simplifies the output, directing the developer to the actual failing line of test code:

```
Test 'CalculatorTests.ShouldAdd' failed:
    Expected 999 but was 5.
    CalculatorTests.cs(14,0): at CalculatorTests.ShouldAdd()

Test 'CalculatorTests.ShouldSubtract' failed:
    Expected 999 but was 2.
    CalculatorTests.cs(19,0): at CalculatorTests.ShouldSubtract()
```

In addition to identifying the types which make up your assertion library of choice, your custom convention may also list assertion extension classes defined in your own projects, further simplifying your output during failures.