using System;
using Xunit;

namespace Fixie.Tests
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
                typeof(Interface)
            };
        }

        [Fact]
        public void ShouldFilterToConcreteClassesByDefault()
        {
            new ClassFilter()
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor), typeof(NoDefaultConstructor), typeof(String));
        }

        [Fact]
        public void ShouldFilterByAllSpecifiedConditions()
        {
            new ClassFilter()
                .Where(type => type.Namespace == "Fixie.Tests")
                .Where(type => type.Name.StartsWith("No"))
                .Filter(candidateTypes)
                .ShouldEqual(typeof(NoDefaultConstructor));
        }

        [Fact]
        public void CanFilterToClassesWithDefaultConstructors()
        {
            new ClassFilter()
                .HasDefaultConstructor()
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor));
        }

        [Fact]
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
    }
}