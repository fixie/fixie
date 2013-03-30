using System;
using Xunit;

namespace Fixie.Tests
{
    public class TypeFilterTests
    {
        readonly Type[] candidateTypes;

        public TypeFilterTests()
        {
            candidateTypes = new[]
            {
                typeof(Decimal),
                typeof(AbstractClass),
                typeof(DateTime),
                typeof(DefaultConstructor),
                typeof(NoDefaultConstructor),
                typeof(String),
                typeof(Interface)
            };
        }

        [Fact]
        public void ShouldIncludeAllTypesByDefault()
        {
            new TypeFilter()
                .Filter(candidateTypes)
                .ShouldEqual(candidateTypes);
        }

        [Fact]
        public void ShouldFilterByAllSpecifiedConditions()
        {
            new TypeFilter()
                .Where(type => type.Namespace == "System")
                .Where(type => type.Name.StartsWith("D"))
                .Filter(candidateTypes)
                .ShouldEqual(typeof(Decimal), typeof(DateTime));
        }

        [Fact]
        public void CanFilterToConcreteClasses()
        {
            new TypeFilter()
                .ConcreteClasses()
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor), typeof(NoDefaultConstructor), typeof(String));
        }

        [Fact]
        public void CanFilterToClassesWithDefaultConstructors()
        {
            new TypeFilter()
                .HasDefaultConstructor()
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor));
        }

        [Fact]
        public void CanFilterByTypeNameSuffix()
        {
            new TypeFilter()
                .NameEndsWith("Constructor")
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor), typeof(NoDefaultConstructor));
        }

        abstract class AbstractClass { }
        class DefaultConstructor { }
        class NoDefaultConstructor { public NoDefaultConstructor(int arg) { } }
        interface Interface { }
    }
}