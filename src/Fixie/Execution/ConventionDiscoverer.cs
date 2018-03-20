namespace Fixie.Execution
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Cli;
    using Conventions;

    class ConventionDiscoverer
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
            if (assembly.GetName().Name == "Fixie.Tests")
            {
                return new[] { (Convention)new DefaultConvention() };
            }

            var customConventions =
                LocallyDeclaredConventionTypes()
                    .Select(ConstructConvention)
                    .ToArray();

            if (customConventions.Any())
                return customConventions;

            return new[] { (Convention)new DefaultConvention() };
        }

        Type[] LocallyDeclaredConventionTypes()
        {
            return assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Convention)) && !t.IsAbstract)
                .ToArray();
        }

        Convention ConstructConvention(Type type)
        {
            try
            {
                return (Convention)CommandLine.Parse(type, conventionArguments);
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
    }
}