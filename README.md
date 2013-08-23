# Fixie

Fixie is a .NET convention-based test framework similar to NUnit and xUnit, but with an emphasis on low-ceremony defaults and flexible customization. Fixie's development is documented at [http://plioi.github.io/fixie](http://plioi.github.io/fixie).

## How do I install Fixie?

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install the [Fixie NuGet package](https://www.nuget.org/packages/Fixie) from the package manager console:

    PM> Install-Package Fixie

## How do I use Fixie?

1. Create a Class Library project to house your test classes.
2. Add a reference to Fixie.dll.
3. Add test classes and test cases to your testing project.
4. Use the console runner from a command line to execute your tests:

    Fixie.Console.exe path/to/your/test/project.dll
5. Use the TestDriven.NET runner from within Visual Studio, using the same keyboard shortcuts you would use for NUnit tests.

## Default Convention

When using the default convention, a test class is any concrete class in your test assembly whose name ending in "Tests".  Within such a test class, a test case is any public instance void method.  Additionally, test cases include public instance async methods returning `Task` or `Task<T>`.

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

## Custom Conventions

Although useful for simple scenarios, the default convention may inadequate. Fortunately, you can replace it with your own.

If you don't want to go with the behaviors defined in the default convention, simply place a subclass of Convention beside your tests.  A custom subclass of Convention will reach out into the containing test assembly, looking for tests to execute.  Each convention can customize test discovery and test execution.  For test discovery, you describe what your test classes and test methods look like.  For test execution, you can take control over how frequently your test classes are constructed and how they are constructed.  Additionally, you can wrap custom behavior around each test method, around each test class instance, and around each test class.

For instance, let's say we want all of our integration tests to be automatically wrapped in a database transaction.  Beside our tests, we place a custom convention class:

```cs
using Fixie;
using Fixie.Conventions;

namespace IntegrationTests
{
    public class IntegrationTestConvention : Convention
    {
        public IntegrationTestConvention()
        {
            Classes
                .NameEndsWith("Tests");

            Cases
                .Where(method => method.Void());

            InstanceExecution
                .Wrap((fixture, innerBehavior) =>
                {
                    using (new TransactionScope())
                        innerBehavior();
                });
        }
    }
}
```

Several sample conventions are available under the [Fixie.Samples](https://github.com/plioi/fixie/tree/master/src/Fixie.Samples) project:

* [Imitate NUnit](https://github.com/plioi/fixie/blob/master/src/Fixie.Samples/NUnitStyle/CustomConvention.cs)
* [Imitate xUnit](https://github.com/plioi/fixie/blob/master/src/Fixie.Samples/xUnitStyle/CustomConvention.cs)
* [Simplified NUnit for simpler test inheritance](https://github.com/plioi/fixie/blob/master/src/Fixie.Samples/LowCeremony/CustomConvention.cs)
* [Construct integration test classes with your IoC container](https://github.com/plioi/fixie/blob/master/src/Fixie.Samples/IoC/CustomConvention.cs)
* [Support arbitrary command line flags such as NUnit-style categories](https://github.com/plioi/fixie/blob/master/src/Fixie.Samples/Categories/CustomConvention.cs)

## How do I make assertions?

Most test frameworks such as NUnit or xUnit include their own assertion libraries so that you can make statements like this:

```cs
Assert.AreEqual(expected, actual);
```

Assertion libraries are orthogonal to test frameworks.  Your choice of assertion library should be independent of your choice of test framework.  Therefore, Fixie will *never* include an assertion library.

Here are some useful third-party assertion libraries:

* [Should](http://nuget.org/packages/Should/)
* [Shouldly](http://nuget.org/packages/Shouldly/)

