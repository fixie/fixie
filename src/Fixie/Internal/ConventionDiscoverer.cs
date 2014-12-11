using System;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Internal
{
    public class ConventionDiscoverer
    {
        readonly RunContext runContext;

        public ConventionDiscoverer(RunContext runContext)
        {
            this.runContext = runContext;
        }

        public Convention[] GetConventions()
        {
            var testAssemblyTypes = ConcreteTestAssemblyTypes();

            var conventionTypes = testAssemblyTypes.Any()
                ? ExplicitlyAppliedConventionTypes(testAssemblyTypes)
                : LocallyDeclaredConventionTypes();

            var customConventions =
                conventionTypes
                    .Select(t => Construct<Convention>(t, runContext))
                    .ToArray();

            if (customConventions.Any())
                return customConventions;

            return new[] { (Convention)new DefaultConvention() };
        }

        Type[] ConcreteTestAssemblyTypes()
        {
            return runContext.Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(TestAssembly)) && !t.IsAbstract)
                .ToArray();
        }

        Type[] ExplicitlyAppliedConventionTypes(Type[] testAssemblyTypes)
        {
            return testAssemblyTypes
                .Select(t => Construct<TestAssembly>(t, runContext))
                .SelectMany(x => x.ConventionTypes)
                .ToArray();
        }

        Type[] LocallyDeclaredConventionTypes()
        {
            return runContext.Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Convention)) && !t.IsAbstract)
                .ToArray();
        }

        static T Construct<T>(Type type, RunContext runContext)
        {
            var constructor = GetConstructor(type);

            try
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length == 1 && parameters.Single().ParameterType == typeof(RunContext))
                    return (T)constructor.Invoke(new object[] { runContext });

                return (T)constructor.Invoke(null);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Could not construct an instance of type '{0}'.", type.FullName), ex);
            }
        }

        static ConstructorInfo GetConstructor(Type type)
        {
            var constructors = type.GetConstructors();

            if (constructors.Length == 1)
                return constructors.Single();

            throw new Exception(
                String.Format("Could not construct an instance of type '{0}'.  Expected to find exactly 1 public constructor, but found {1}.",
                    type.FullName, constructors.Length));
        }
    }
}