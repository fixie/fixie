namespace Fixie.Execution
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Conventions;

    public class ConventionDiscoverer
    {
        readonly Assembly assembly;

        public ConventionDiscoverer(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public Convention[] GetConventions()
        {
            var testAssemblyTypes = ConcreteTestAssemblyTypes();

            var conventionTypes = testAssemblyTypes.Any()
                ? ExplicitlyAppliedConventionTypes(testAssemblyTypes)
                : LocallyDeclaredConventionTypes();

            var customConventions =
                conventionTypes
                    .Select(Construct<Convention>)
                    .ToArray();

            if (customConventions.Any())
                return customConventions;

            return new[] { (Convention)new DefaultConvention() };
        }

        Type[] ConcreteTestAssemblyTypes()
        {
            return assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(TestAssembly)) && !t.IsAbstract)
                .ToArray();
        }

        Type[] ExplicitlyAppliedConventionTypes(Type[] testAssemblyTypes)
        {
            return testAssemblyTypes
                .Select(Construct<TestAssembly>)
                .SelectMany(x => x.ConventionTypes)
                .ToArray();
        }

        Type[] LocallyDeclaredConventionTypes()
        {
            return assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Convention)) && !t.IsAbstract)
                .ToArray();
        }

        static T Construct<T>(Type type)
        {
            var constructor = GetConstructor(type);

            try
            {
                return (T)constructor.Invoke(null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not construct an instance of type '{type.FullName}'.", ex);
            }
        }

        static ConstructorInfo GetConstructor(Type type)
        {
            var constructors = type.GetConstructors();

            if (constructors.Length == 1)
                return constructors.Single();

            throw new Exception(
                $"Could not construct an instance of type '{type.FullName}'.  Expected to find exactly 1 public constructor, but found {constructors.Length}.");
        }
    }
}