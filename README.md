# Fixie

Fixie is a .NET convention-based test framework similar to NUnit and xUnit, but with an emphasis on low-ceremony defaults and flexible customization. Fixie's development is documented at [http://plioi.github.io/fixie](http://plioi.github.io/fixie).

## How do I install Fixie?

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install the [Fixie NuGet package](https://www.nuget.org/packages/Fixie) from the package manager console:

    PM> Install-Package Fixie

## How do I use Fixie?

1. Create a Class Library project to house your test classes.
2. Add a reference to Fixie.dll.
3. Add test classes and test cases to your testing project.
4. Use the console runner (should be at `your-solution-dir/packages/Fixie.x.x.x/lib/netXX/`) from a command line to execute your tests:

    `Fixie.Console.exe path/to/your/test/project.dll`
    
5. Use the TestDriven.NET runner from within Visual Studio, using the same keyboard shortcuts you would use for NUnit tests.

## Documentation

1. [Default Convention](doc/DefaultConvention.md)
2. [Custom Conventions](doc/CustomConventions.md)
3. [Parameterized Test Methods](doc/ParameterizedTestMethods.md)
4. [Skipping Tests](doc/SkippingTests.md)
5. [Assertions](doc/Assertions.md)
6. [Continuous Integration](doc/ContinuousIntegration.md)