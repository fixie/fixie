using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Fixie.Conventions;
using Fixie.Internal;
using Should;

namespace Fixie.Tests.Internal
{
    public class ClassDiscovererTests
    {
        static readonly Type[] CandidateTypes =
        {
            typeof(Decimal),
            typeof(AbstractClass),
            typeof(DateTime),
            typeof(DefaultConstructor),
            typeof(NoDefaultConstructor),
            typeof(NameEndsWithTests),
            typeof(String),
            typeof(Interface),
            typeof(AttributeSampleBase),
            typeof(AttributeSample)
        };
        
        public void ShouldConsiderOnlyConcreteClasses()
        {
            var customConvention = new Convention();

            DiscoveredTestClasses(customConvention)
                .ShouldEqual(
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor),
                    typeof(NameEndsWithTests),
                    typeof(String),
                    typeof(AttributeSampleBase),
                    typeof(AttributeSample));
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
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor),
                    typeof(NameEndsWithTests),
                    typeof(String),
                    typeof(AttributeSampleBase),
                    typeof(AttributeSample));
        }

        public void ShouldDiscoverClassesSatisfyingAllSpecifiedConditions()
        {
            var customConvention = new Convention();

            customConvention
                .Classes
                .Where(type => type.IsInNamespace("Fixie.Tests"))
                .Where(type => type.Name.StartsWith("No"));

            DiscoveredTestClasses(customConvention)
                .ShouldEqual(typeof(NoDefaultConstructor));
        }

        public void CanDiscoverClassesByNonInheritedAttributes()
        {
            var customConvention = new Convention();

            customConvention
                .Classes
                .Has<NonInheritedAttribute>();

            DiscoveredTestClasses(customConvention)
                .ShouldEqual(typeof(AttributeSample));
        }

        public void CanDiscoverClassesByInheritedAttributes()
        {
            var customConvention = new Convention();

            customConvention
                .Classes
                .HasOrInherits<InheritedAttribute>();

            DiscoveredTestClasses(customConvention)
                .ShouldEqual(
                    typeof(AttributeSampleBase),
                    typeof(AttributeSample));
        }

        public void CanDiscoverClassesByTypeNameSuffixes()
        {
            var convention = new Convention();

            convention
                .Classes
                .NameEndsWith("Constructor", "Sample");

            DiscoveredTestClasses(convention)
                .ShouldEqual(
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor),
                    typeof(AttributeSample));
        }

        public void CanDiscoverClassesInTheSameNamespaceAsSpecifiedType()
        {
            var convention = new Convention();

            convention
                .Classes
                .InTheSameNamespaceAs(typeof(DefaultConstructor));

            DiscoveredTestClasses(convention, typeof(NestedNamespace.InNestedNamespace))
                .ShouldEqual(
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor),
                    typeof(NameEndsWithTests),
                    typeof(AttributeSampleBase),
                    typeof(AttributeSample),
                    typeof(NestedNamespace.InNestedNamespace));
        }

        public void CanDiscoverClassesInAnyOfTheSpecifiedNamespaces()
        {
            var convention = new Convention();

            convention
                .Classes
                .InTheSameNamespaceAs(typeof(DefaultConstructor), typeof(DateTime));

            DiscoveredTestClasses(convention, typeof(NestedNamespace.InNestedNamespace))
                .ShouldEqual(
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor),
                    typeof(NameEndsWithTests),
                    typeof(String),
                    typeof(AttributeSampleBase),
                    typeof(AttributeSample),
                    typeof(NestedNamespace.InNestedNamespace));
        }

        public void DoesNotMindIfMultipleTypesPointToSameNamespace()
        {
            var convention = new Convention();

            convention
                .Classes
                .InTheSameNamespaceAs(typeof(DefaultConstructor), typeof(NestedNamespace.InNestedNamespace), typeof(NoDefaultConstructor));

            DiscoveredTestClasses(convention, typeof(NestedNamespace.InNestedNamespace))
                .ShouldEqual(
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor),
                    typeof(NameEndsWithTests),
                    typeof(AttributeSampleBase),
                    typeof(AttributeSample),
                    typeof(NestedNamespace.InNestedNamespace));
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
                .Where(type => { throw new Exception("Unsafe class-discovery predicate threw!"); });

            Action attemptFaultyDiscovery = () => DiscoveredTestClasses(customConvention);

            var exception = attemptFaultyDiscovery.ShouldThrow<Exception>(
                "Exception thrown while attempting to run a custom class-discovery predicate. " +
                "Check the inner exception for more details.");

            exception.InnerException.Message.ShouldEqual("Unsafe class-discovery predicate threw!");
        }

        static IEnumerable<Type> DiscoveredTestClasses(Convention convention)
        {
            return new ClassDiscoverer(convention.Config)
                .TestClasses(CandidateTypes);
        }

        static IEnumerable<Type> DiscoveredTestClasses(Convention convention, params Type[] additionalCandidates)
        {
            return new ClassDiscoverer(convention.Config)
                .TestClasses(CandidateTypes.Concat(additionalCandidates));
        }

        abstract class AbstractClass { }
        class DefaultConstructor { }
        class NoDefaultConstructor { public NoDefaultConstructor(int arg) { } }
        class NameEndsWithTests { }
        interface Interface { }

        class InheritedAttribute : Attribute { }
        class NonInheritedAttribute : Attribute { }

        [Inherited]
        class AttributeSampleBase { }

        [NonInherited]
        class AttributeSample : AttributeSampleBase { }

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

namespace Fixie.Tests.Internal.NestedNamespace
{
    class InNestedNamespace { }
}