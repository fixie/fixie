namespace Fixie.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Internal;

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

        class DefaultDiscovery : Discovery
        {
        }

        class MaximumDiscovery : Discovery
        {
            public MaximumDiscovery()
            {
                //Include a trivial condition, causing the default "name ends with 'Tests'" rule
                //to be suppressed in favor of only the rules specified here in ClassDiscovererTests.
                TestClassConditions.Add(x => true);
            }
        }

        class NarrowDiscovery : Discovery
        {
            public NarrowDiscovery()
            {
                TestClassConditions.Add(x => (x.Namespace ?? "").StartsWith("Fixie.Tests"));
                TestClassConditions.Add(x => x.Name.Contains("i"));
                TestClassConditions.Add(x => !x.IsStatic());
            }
        }
        
        class BuggyDiscovery : Discovery
        {
            public BuggyDiscovery()
                => TestClassConditions.Add(x => throw new Exception("Unsafe class-discovery predicate threw!"));
        }

        class SampleExecution : Execution
        {
            public void Execute(TestClass testClass)
            {
            }
        }
        
        public void ShouldConsiderOnlyConcreteClasses()
        {
            var discovery = new MaximumDiscovery();

            DiscoveredTestClasses(discovery)
                .ShouldBe(
                    typeof(StaticClass),
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor),
                    typeof(NameEndsWithTests),
                    typeof(String),
                    typeof(InheritanceSampleBase),
                    typeof(InheritanceSample));
        }

        public void ShouldNotConsiderDiscoveryAndExecutionCustomizationClasses()
        {
            var discovery = new MaximumDiscovery();

            DiscoveredTestClasses(discovery,
                    typeof(DefaultDiscovery),
                    typeof(MaximumDiscovery),
                    typeof(NarrowDiscovery),
                    typeof(BuggyDiscovery),
                    typeof(SampleExecution))
                .ShouldBe(
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
            nested.Has<CompilerGeneratedAttribute>().ShouldBe(true);

            //Confirm that the nested closure class is omitted from test class discovery.
            var discovery = new MaximumDiscovery();

            DiscoveredTestClasses(discovery, nested)
                .ShouldBe(
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
            var discovery = new NarrowDiscovery();

            DiscoveredTestClasses(discovery)
                .ShouldBe(
                    typeof(NameEndsWithTests),
                    typeof(InheritanceSampleBase),
                    typeof(InheritanceSample));
        }

        public void TheDefaultDiscoveryShouldDiscoverClassesWhoseNameEndsWithTests()
        {
            var discovery = new DefaultDiscovery();

            DiscoveredTestClasses(discovery)
                .ShouldBe(
                    typeof(NameEndsWithTests));
        }

        public void ShouldFailWithClearExplanationWhenAnyGivenConditionThrows()
        {
            var discovery = new BuggyDiscovery();

            Action attemptFaultyDiscovery = () => DiscoveredTestClasses(discovery);

            var exception = attemptFaultyDiscovery.ShouldThrow<Exception>(
                "Exception thrown while attempting to run a custom class-discovery predicate. " +
                "Check the inner exception for more details.");

            exception.InnerException
                .ShouldBe<Exception>()
                .Message.ShouldBe("Unsafe class-discovery predicate threw!");
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