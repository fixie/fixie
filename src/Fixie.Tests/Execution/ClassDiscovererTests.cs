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
        
        public void ShouldConsiderOnlyConcreteClasses()
        {
            var customConvention = new Convention();

            DiscoveredTestClasses(customConvention)
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
            var customConvention = new Convention();

            DiscoveredTestClasses(customConvention, nested)
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
            var customConvention = new Convention();

            customConvention
                .Classes
                .Where(x => x.IsInNamespace("Fixie.Tests"))
                .Where(x => x.Name.Contains("i"))
                .Where(x => !x.IsStatic());

            DiscoveredTestClasses(customConvention)
                .ShouldEqual(
                    typeof(NameEndsWithTests),
                    typeof(InheritanceSampleBase),
                    typeof(InheritanceSample));
        }

        public void TheDefaultConventionShouldDiscoverClassesWhoseNameEndsWithTests()
        {
            var defaultConvention = new DefaultConvention();

            DiscoveredTestClasses(defaultConvention)
                .ShouldEqual(
                    typeof(NameEndsWithTests));
        }

        public void ShouldFailWithClearExplanationWhenAnyGivenConditionThrows()
        {
            var customConvention = new Convention();

            customConvention
                .Classes
                .Where(x => throw new Exception("Unsafe class-discovery predicate threw!"));

            Action attemptFaultyDiscovery = () => DiscoveredTestClasses(customConvention);

            var exception = attemptFaultyDiscovery.ShouldThrow<Exception>(
                "Exception thrown while attempting to run a custom class-discovery predicate. " +
                "Check the inner exception for more details.");

            exception.InnerException.Message.ShouldEqual("Unsafe class-discovery predicate threw!");
        }

        static IEnumerable<Type> DiscoveredTestClasses(Convention convention)
        {
            return new ClassDiscoverer(convention)
                .TestClasses(CandidateTypes);
        }

        static IEnumerable<Type> DiscoveredTestClasses(Convention convention, params Type[] additionalCandidates)
        {
            return new ClassDiscoverer(convention)
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