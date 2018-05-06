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

        public Convention GetConvention()
        {
            if (assembly.GetName().Name == "Fixie.Tests")
                return new DefaultConvention();

            var locallyDeclaredConventionTypes = LocallyDeclaredConventionTypes();

            if (locallyDeclaredConventionTypes.Length > 1)
            {
                throw new Exception(
                    "A test assembly should have at most one convention, " +
                    "but the following conventions were discovered:" + Environment.NewLine +
                    String.Join(Environment.NewLine,
                        locallyDeclaredConventionTypes
                            .Select(x => $"\t{x.FullName}")));
            }

            if (!locallyDeclaredConventionTypes.Any())
                return new DefaultConvention();

            return ConstructConvention(locallyDeclaredConventionTypes.Single());
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