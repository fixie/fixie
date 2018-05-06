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
        readonly string[] customArguments;

        public ConventionDiscoverer(Assembly assembly, string[] customArguments)
        {
            this.assembly = assembly;
            this.customArguments = customArguments;
        }

        public Discovery GetDiscovery()
        {
            return (Discovery) Construct(DiscoveryType());
        }

        public void GetConvention(out Discovery discovery, out Lifecycle lifecycle)
        {
            var discoveryType = DiscoveryType();
            var lifecycleType = LifecycleType();

            discovery = (Discovery) Construct(discoveryType);
            lifecycle = discoveryType == lifecycleType
                ? (Lifecycle) discovery
                : (Lifecycle) Construct(lifecycleType);
        }

        Type DiscoveryType()
        {
            if (assembly.GetName().Name == "Fixie.Tests")
                return typeof(DefaultDiscovery);

            var customDiscoveryTypes = assembly
                .GetTypes()
                .Where(type => IsDiscovery(type) && !type.IsAbstract)
                .ToArray();

            if (customDiscoveryTypes.Length > 1)
            {
                throw new Exception(
                    "A test assembly can have at most one Discovery implementation, " +
                    "but the following implementations were discovered:" + Environment.NewLine +
                    String.Join(Environment.NewLine,
                        customDiscoveryTypes
                            .Select(x => $"\t{x.FullName}")));
            }

            if (customDiscoveryTypes.Any())
                return customDiscoveryTypes.Single();

            return typeof(DefaultDiscovery);
        }

        Type LifecycleType()
        {
            if (assembly.GetName().Name == "Fixie.Tests")
                return typeof(DefaultLifecycle);

            var customLifecycleTypes = assembly
                .GetTypes()
                .Where(type => IsLifecycle(type) && !type.IsAbstract)
                .ToArray();

            if (customLifecycleTypes.Length > 1)
            {
                throw new Exception(
                    "A test assembly can have at most one Lifecycle implementation, " +
                    "but the following implementations were discovered:" + Environment.NewLine +
                    String.Join(Environment.NewLine,
                        customLifecycleTypes
                            .Select(x => $"\t{x.FullName}")));
            }

            if (customLifecycleTypes.Any())
                return customLifecycleTypes.Single();

            return typeof(DefaultLifecycle);
        }

        static bool IsDiscovery(Type type)
            => type.IsSubclassOf(typeof(Discovery));

        static bool IsLifecycle(Type type)
            => type.GetInterfaces().Contains(typeof(Lifecycle));

        object Construct(Type type)
        {
            try
            {
                return CommandLine.Parse(type, customArguments);
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