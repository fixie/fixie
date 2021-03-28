namespace Fixie.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
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

        class MaximumDiscovery : Discovery
        {
            public IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses)
                => concreteClasses;

            public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
                => throw new ShouldBeUnreachableException();
        }

        class NarrowDiscovery : Discovery
        {
            public IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses)
            {
                return concreteClasses
                    .Where(x => (x.Namespace ?? "").StartsWith("Fixie.Tests"))
                    .Where(x => x.Name.Contains("i"))
                    .Where(x => !x.IsStatic());
            }

            public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
                => throw new ShouldBeUnreachableException();
        }
        
        class BuggyDiscovery : Discovery
        {
            public IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses)
            {
                return concreteClasses.Where(x => throw new Exception("Unsafe class-discovery predicate threw!"));
            }

            public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
                => throw new ShouldBeUnreachableException();
        }

        class SampleExecution : Execution
        {
            public Task RunAsync(TestAssembly testAssembly)
                => Task.CompletedTask;
        }

        public void TheDefaultDiscoveryShouldDiscoverClassesWhoseNameEndsWithTests()
        {
            var discovery = new DefaultDiscovery();

            DiscoveredTestClasses(discovery)
                .ShouldBe(
                    typeof(NameEndsWithTests));
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

        public void ShouldFailWithClearExplanationWhenDiscoveryThrows()
        {
            var discovery = new BuggyDiscovery();

            Action attemptFaultyDiscovery = () => DiscoveredTestClasses(discovery);

            var exception = attemptFaultyDiscovery.ShouldThrow<Exception>(
                "Exception thrown during test class discovery. " +
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