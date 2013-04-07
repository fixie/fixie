# Fixie

Fixie is a .NET convention-based test framework similar to NUnit and xUnit, but with an emphasis on low-ceremony defaults and flexible customization.

## How do I install Fixie?

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install Fixie from the package manager console:

    PM> Install-Package Fixie

## How do I use Fixie?

1. Create a Class Library project to house your test fixtures.
2. Add a reference to Fixie.dll.
3. Add fixture classes and test cases to your testing project.
4. Use the console runner from a command line to execute your tests:

    Fixie.Console.exe path/to/your/test/project.dll
5. Use the TestDriven.NET runner from within Visual Studio, using the same keyboard shortcuts you would use for NUnit tests.

## Default Convention

When using the default convention, a test fixture is any concrete class that has a default constructor and a name ending in "Tests".  Within such a fixture class, a test case is any public instance void method with zero arguments.  If you want to perform setup steps before each test case executes, you can place that in the fixture's default constructor.  One instance of your fixture class is constructed for *each* test case.

No [Attributes], no "using Fixie;" statement, no muss, no fuss.

## How do I make assertions?

Most test frameworks such as NUnit or xUnit include their own assertion libraries so that you can make statements like this:

```cs
Assert.AreEqual(expected, actual);
```

Assertion libraries are orthogonal to test frameworks.  Your choice of assertion library should be independent of your choice of test framework.  Therefore, Fixie will *never* include an assertion library.

Here are some useful third-party assertion libraries:

* [Should](http://nuget.org/packages/Should/)
* [Shouldly](http://nuget.org/packages/Shouldly/)

## Example
```cs
using Should;

public class CalculatorTests
{
    Calculator calculator;

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
}
```
