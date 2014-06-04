## Custom Conventions

Although useful for simple scenarios, the default convention may not meet your needs. Fortunately, you can replace it with your own.

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

            Methods
                .Where(method => method.IsVoid());

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
* [Simplified NUnit for cleaner test inheritance](https://github.com/plioi/fixie/blob/master/src/Fixie.Samples/LowCeremony/CustomConvention.cs)
* [Construct integration test classes with your IoC container](https://github.com/plioi/fixie/blob/master/src/Fixie.Samples/IoC/CustomConvention.cs)
* [Support arbitrary command line flags such as NUnit-style categories](https://github.com/plioi/fixie/blob/master/src/Fixie.Samples/Categories/CustomConvention.cs)

## Sharing Conventions

As described above, a custom subclass of Convention will reach out into the containing test assembly, looking for tests to execute.  This default behavior is useful in simple projects, but is insufficient in two scenarios:

1. You may want to define a convention once in your Solution, and have it applied to multiple test assemblies in that Solution.
2. Someone may create a useful NUnit-mimicking convention and share it in the form of a DLL.

By default, conventions only reach out into their containing assembly when looking for tests.  In order to take advantage of a convention defined in another assembly, you can add a subclass of TestAssembly and explicitly list which conventions apply here:

```cs
using Fixie.Conventions;
using SomeThirdPartyConventionsLibrary;

namespace IntegrationTests
{
    public class IntegrationTestAssembly : TestAssembly
    {
        public IntegrationTestAssembly ()
        {
            Apply<NUnitConvention>();
            Apply<xUnitConvention>();
        }
    }
}
```
