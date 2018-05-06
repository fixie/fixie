namespace Fixie.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Conventions;
    using Fixie.Execution;

    public class ClassDiscovererTests
    {
        static readonly Type[] CandidateTypes =
        {
            typeof(Decimal),
            typeof(StaticClass),
            typeof(AbstractClass),
            typeof(DateTime),
            typeof(DefaultConstructor),
            typeof(NoDefaultConstructor),
            typeof(NameEndsWithTests),
            typeof(String),
            typeof(Interface),
            typeof(InheritanceSampleBase),
            typeof(InheritanceSample)
        };

        class SampleDiscovery : Discovery
        {
        }

        class SampleLifecycle : Lifecycle
        {
            public void Execute(TestClass testClass)
            {
            }
        }
        
        public void ShouldConsiderOnlyConcreteClasses()
        {
            var customDiscovery = new SampleDiscovery();

            DiscoveredTestClasses(customDiscovery)
                .ShouldEqual(
                    typeof(StaticClass),
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor),
                    typeof(NameEndsWithTests),
                    typeof(String),
                    typeof(InheritanceSampleBase),
                    typeof(InheritanceSample));
        }

        public void ShouldNotConsiderDiscoveryAndLifecycleCustomizationClasses()
        {
            var customDiscovery = new SampleDiscovery();

            DiscoveredTestClasses(customDiscovery,
                    typeof(SampleDiscovery),
                    typeof(SampleLifecycle))
                .ShouldEqual(
                    typeof(StaticClass),
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor),
                    typeof(NameEndsWithTests),
                    typeof(String),
                    typeof(InheritanceSampleBase),
                    typeof(InheritanceSample));
        }

        public void ShouldNotConsiderCompilerGeneratedClosureClasses()
        {
            var nested = typeof(ClassThatCausesCompilerGeneratedNestedClosureClass)
                .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Single();

            //Confirm that a nested closure class has actually been generated.
            nested.Has<CompilerGeneratedAttribute>().ShouldBeTrue();

            //Confirm that the nested closure class is omitted from test class discovery.
            var customDiscovery = new SampleDiscovery();

            DiscoveredTestClasses(customDiscovery, nested)
                .ShouldEqual(
                    typeof(StaticClass),
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor),
                    typeof(NameEndsWithTests),
                    typeof(String),
                    typeof(InheritanceSampleBase),
                    typeof(InheritanceSample));
        }

        public void ShouldDiscoverClassesSatisfyingAllSpecifiedConditions()
        {
            var customDiscovery = new SampleDiscovery();

            customDiscovery
                .Classes
                .Where(x => x.IsInNamespace("Fixie.Tests"))
                .Where(x => x.Name.Contains("i"))
                .Where(x => !x.IsStatic());

            DiscoveredTestClasses(customDiscovery)
                .ShouldEqual(
                    typeof(NameEndsWithTests),
                    typeof(InheritanceSampleBase),
                    typeof(InheritanceSample));
        }

        public void TheDefaultDiscoveryShouldDiscoverClassesWhoseNameEndsWithTests()
        {
            var defaultDiscovery = new DefaultDiscovery();

            DiscoveredTestClasses(defaultDiscovery)
                .ShouldEqual(
                    typeof(NameEndsWithTests));
        }

        public void ShouldFailWithClearExplanationWhenAnyGivenConditionThrows()
        {
            var customDiscovery = new SampleDiscovery();

            customDiscovery
                .Classes
                .Where(x => throw new Exception("Unsafe class-discovery predicate threw!"));

            Action attemptFaultyDiscovery = () => DiscoveredTestClasses(customDiscovery);

            var exception = attemptFaultyDiscovery.ShouldThrow<Exception>(
                "Exception thrown while attempting to run a custom class-discovery predicate. " +
                "Check the inner exception for more details.");

            exception.InnerException.Message.ShouldEqual("Unsafe class-discovery predicate threw!");
        }

        static IEnumerable<Type> DiscoveredTestClasses(Discovery discovery)
        {
            return new ClassDiscoverer(discovery)
                .TestClasses(CandidateTypes);
        }

        static IEnumerable<Type> DiscoveredTestClasses(Discovery discovery, params Type[] additionalCandidates)
        {
            return new ClassDiscoverer(discovery)
                .TestClasses(CandidateTypes.Concat(additionalCandidates));
        }

        static class StaticClass { }
        abstract class AbstractClass { }
        class DefaultConstructor { }
        class NoDefaultConstructor { public NoDefaultConstructor(int arg) { } }
        class NameEndsWithTests { }
        interface Interface { }

        class InheritanceSampleBase { }

        class InheritanceSample : InheritanceSampleBase { }

        class ClassThatCausesCompilerGeneratedNestedClosureClass
        {
            void MethodThatCausesCompilerGeneratedClosureClass()
            {
                //Because this lambda expression refers to a variable in the surrounding
                //method, the compiler generates a special nested class as an implementation
                //detail.  Such generated classes should never be mistaken for test classes.

                var s = string.Empty;
                var func = new Func<string, string>(_ => s);
            }
        }
    }
}