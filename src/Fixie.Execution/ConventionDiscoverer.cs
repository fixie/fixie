namespace Fixie.Execution
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Cli;
    using Conventions;

    public class ConventionDiscoverer
    {
        readonly Assembly assembly;
        readonly string[] conventionArguments;

        public ConventionDiscoverer(Assembly assembly, string[] conventionArguments)
        {
            this.assembly = assembly;
            this.conventionArguments = conventionArguments;
        }

        public Convention[] GetConventions()
        {
            var customConventions =
                LocallyDeclaredConventionTypes()
                    .Select(Construct<Convention>)
                    .ToArray();

            if (customConventions.Any())
                return customConventions;

            return new[] { (Convention)new DefaultConvention() };
        }

        Type[] LocallyDeclaredConventionTypes()
        {
            return assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Convention)) && !t.IsAbstract())
                .ToArray();
        }

        T Construct<T>(Type type)
        {
            var constructor = GetConstructor(type);

            try
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length == 1)
                {
                    var options = CommandLine.Parse(parameters.Single().ParameterType, conventionArguments);

                    return (T)constructor.Invoke(new[] {options});
                }

                return (T)constructor.Invoke(null);
            }
            catch (CommandLineException ex)
            {
                throw new Exception($"Command line argument parsing failed while attempting to construct an instance of type '{type.FullName}'. " + ex.Message);
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