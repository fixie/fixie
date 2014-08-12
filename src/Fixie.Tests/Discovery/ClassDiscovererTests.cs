using System;
using System.Collections.Generic;
using Should;
using Fixie.Conventions;
using Fixie.Discovery;

namespace Fixie.Tests.Discovery
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

        public void CanDiscoverMethodsByNonInheritedAttributes()
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

        public void CanDiscoverClassesByTypeNameSuffix()
        {
            var convention = new Convention();

            convention
                .Classes
                .NameEndsWith("Constructor");

            DiscoveredTestClasses(convention)
                .ShouldEqual(
                    typeof(DefaultConstructor),
                    typeof(NoDefaultConstructor));
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
    }
}