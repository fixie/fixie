using System;
using System.Collections.Generic;
using Fixie.Conventions;

namespace Fixie.Tests.Conventions
{
    public class TestClassExpressionTests
    {
        readonly Convention convention;
        readonly Type[] candidateTypes;

        public TestClassExpressionTests()
        {
            candidateTypes = new[]
            {
                typeof(Decimal),
                typeof(AbstractClass),
                typeof(DateTime),
                typeof(DefaultConstructor),
                typeof(NoDefaultConstructor),
                typeof(String),
                typeof(Interface),
                typeof(AttributeSampleBase),
                typeof(AttributeSample)
            };

            convention = new Convention();
        }

        public void ShouldFilterToConcreteClassesByDefault()
        {
            DiscoveredTestClasses()
                .ShouldEqual(typeof(DefaultConstructor), typeof(NoDefaultConstructor), typeof(String),
                    typeof(AttributeSampleBase), typeof(AttributeSample));
        }

        public void ShouldFilterByAllSpecifiedConditions()
        {
            convention
                .Classes
                .Where(type => type.IsInNamespace("Fixie.Tests"))
                .Where(type => type.Name.StartsWith("No"));

            DiscoveredTestClasses()
                .ShouldEqual(typeof(NoDefaultConstructor));
        }

        public void CanFilterToClassesWithNonInheritedAttributes()
        {
            convention
                .Classes
                .Has<NonInheritedAttribute>();

            DiscoveredTestClasses()
                .ShouldEqual(typeof(AttributeSample));
        }

        public void CanFilterToClassesWithInheritedAttributes()
        {
            convention
                .Classes
                .HasOrInherits<InheritedAttribute>();

            DiscoveredTestClasses()
                .ShouldEqual(typeof(AttributeSampleBase), typeof(AttributeSample));
        }

        public void CanFilterByTypeNameSuffix()
        {
            convention
                .Classes
                .NameEndsWith("Constructor");

            DiscoveredTestClasses()
                .ShouldEqual(typeof(DefaultConstructor), typeof(NoDefaultConstructor));
        }

        IEnumerable<Type> DiscoveredTestClasses()
        {
            return new DiscoveryModel(convention.Config)
                .TestClasses(candidateTypes);
        }

        abstract class AbstractClass { }
        class DefaultConstructor { }
        class NoDefaultConstructor { public NoDefaultConstructor(int arg) { } }
        interface Interface { }

        class InheritedAttribute : Attribute { }
        class NonInheritedAttribute : Attribute { }

        [Inherited]
        class AttributeSampleBase { }

        [NonInheritedAttribute]
        class AttributeSample : AttributeSampleBase { }
    }
}