namespace Fixie.Internal
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Cli;

    class BehaviorDiscoverer
    {
        readonly Assembly assembly;
        readonly string[] customArguments;

        public BehaviorDiscoverer(Assembly assembly, string[] customArguments)
        {
            this.assembly = assembly;
            this.customArguments = customArguments;
        }

        public Discovery GetDiscovery()
        {
            return (Discovery) Construct(DiscoveryType());
        }

        public void GetBehaviors(out Discovery discovery, out Execution execution)
        {
            var discoveryType = DiscoveryType();
            var executionType = ExecutionType();

            discovery = (Discovery) Construct(discoveryType);
            execution = discoveryType == executionType
                ? (Execution) discovery
                : (Execution) Construct(executionType);
        }

        Type DiscoveryType()
        {
            if (assembly.GetName().Name == "Fixie.Tests")
                return typeof(Discovery);

            var customDiscoveryTypes = assembly
                .GetTypes()
                .Where(type => IsDiscovery(type) && !type.IsAbstract)
                .ToArray();

            if (customDiscoveryTypes.Length > 1)
            {
                throw new Exception(
                    "A test assembly can have at most one Discovery implementation, " +
                    "but the following implementations were discovered:" + Environment.NewLine +
                    string.Join(Environment.NewLine,
                        customDiscoveryTypes
                            .Select(x => $"\t{x.FullName}")));
            }

            return customDiscoveryTypes.SingleOrDefault() ?? typeof(Discovery);
        }

        Type ExecutionType()
        {
            if (assembly.GetName().Name == "Fixie.Tests")
                return typeof(DefaultExecution);

            var customExecutionTypes = assembly
                .GetTypes()
                .Where(type => IsExecution(type) && !type.IsAbstract)
                .ToArray();

            if (customExecutionTypes.Length > 1)
            {
                throw new Exception(
                    "A test assembly can have at most one Execution implementation, " +
                    "but the following implementations were discovered:" + Environment.NewLine +
                    string.Join(Environment.NewLine,
                        customExecutionTypes
                            .Select(x => $"\t{x.FullName}")));
            }

            return customExecutionTypes.SingleOrDefault() ?? typeof(DefaultExecution);
        }

        static bool IsDiscovery(Type type)
            => type.IsSubclassOf(typeof(Discovery));

        static bool IsExecution(Type type)
            => type.GetInterfaces().Contains(typeof(Execution));

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