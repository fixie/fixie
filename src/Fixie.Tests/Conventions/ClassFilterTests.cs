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
                typeof(Interface)
            };
        }

        public void ShouldFilterToConcreteClassesByDefault()
        {
            new ClassFilter()
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor), typeof(NoDefaultConstructor), typeof(String));
        }

        public void ShouldFilterByAllSpecifiedConditions()
        {
            new ClassFilter()
                .Where(type => type.IsInNamespace("Fixie.Tests"))
                .Where(type => type.Name.StartsWith("No"))
                .Filter(candidateTypes)
                .ShouldEqual(typeof(NoDefaultConstructor));
        }

        public void CanFilterToClassesWithDefaultConstructors()
        {
            new ClassFilter()
                .HasDefaultConstructor()
                .Filter(candidateTypes)
                .ShouldEqual(typeof(DefaultConstructor));
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
    }
}