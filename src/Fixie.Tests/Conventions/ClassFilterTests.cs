using System;
using Fixie.Conventions;

namespace Fixie.Tests.Conventions
{
    public class ClassFilterTests
    {
        readonly Type[] candidateTypes;

        public ClassFilterTests()
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
        }

        public void ShouldFilterToConcreteClassesByDefault()
        {
            new ClassFilter()
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor), typeof(NoDefaultConstructor), typeof(String),
                             typeof(AttributeSampleBase), typeof(AttributeSample));
        }

        public void ShouldFilterByAllSpecifiedConditions()
        {
            new ClassFilter()
                .Where(type => type.IsInNamespace("Fixie.Tests"))
                .Where(type => type.Name.StartsWith("No"))
                .Filter(candidateTypes)
                .ShouldEqual(typeof(NoDefaultConstructor));
        }

        public void CanFilterToClassesWithAttributes()
        {
            new ClassFilter()
                    .Has<NonInheritedAttribute>()
                    .Filter(candidateTypes)
                    .ShouldEqual(typeof(AttributeSample));

            new ClassFilter()
                    .HasOrInherits<InheritedAttribute>()
                    .Filter(candidateTypes)
                    .ShouldEqual(typeof(AttributeSampleBase), typeof(AttributeSample));
        }

        public void CanFilterByTypeNameSuffix()
        {
            new ClassFilter()
                .NameEndsWith("Constructor")
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor), typeof(NoDefaultConstructor));
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