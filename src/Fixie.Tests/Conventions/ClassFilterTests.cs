using System;
using System.Linq;
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
                    .OrderBy(type => type.Name)
                    .ShouldEqual(typeof(AttributeSample), typeof(AttributeSampleBase));
        }

        public void CanFilterByTypeNameSuffix()
        {
            new ClassFilter()
                .NameEndsWith("Constructor")
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor), typeof(NoDefaultConstructor));
        }

        public void CanBeShuffled()
        {
            new ClassFilter()
                .Shuffle(new Random(0))
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor), typeof(NoDefaultConstructor), typeof(String),
                             typeof(AttributeSample), typeof(AttributeSampleBase));

            new ClassFilter()
                .Shuffle(new Random(1))
                .Filter(candidateTypes)
                .ShouldEqual(typeof(AttributeSampleBase), typeof(String), typeof(AttributeSample),
                             typeof(DefaultConstructor), typeof(NoDefaultConstructor));
        }

        public void CanBeSorted()
        {
            new ClassFilter()
                .Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal))
                .Filter(candidateTypes)
                .ShouldEqual(typeof(AttributeSample), typeof(AttributeSampleBase), typeof(DefaultConstructor),
                             typeof(NoDefaultConstructor), typeof(String));

            new ClassFilter()
                .Sort((x, y) => x.Name.Length.CompareTo(y.Name.Length))
                .Filter(candidateTypes)
                .ShouldEqual(typeof(String),
                             typeof(AttributeSample),
                             typeof(DefaultConstructor),
                             typeof(AttributeSampleBase),
                             typeof(NoDefaultConstructor));
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