namespace Fixie.Internal
{
    using System;
    using System.Linq;
    using System.Reflection;

    class BehaviorDiscoverer
    {
        readonly TestContext context;
        readonly Assembly assembly;

        public BehaviorDiscoverer(TestContext context)
        {
            this.context = context;
            assembly = context.Assembly;
        }

        public IDiscovery GetDiscovery()
        {
            return (IDiscovery) Construct(DiscoveryType());
        }

        public void GetBehaviors(out IDiscovery discovery, out IExecution execution)
        {
            var discoveryType = DiscoveryType();
            var executionType = ExecutionType();

            discovery = (IDiscovery) Construct(discoveryType);
            execution = discoveryType == executionType
                ? (IExecution) discovery
                : (IExecution) Construct(executionType);
        }

        Type DiscoveryType()
        {
            var customDiscoveryTypes = assembly
                .GetTypes()
                .Where(type => IsDiscovery(type) && !type.IsAbstract)
                .ToArray();

            if (assembly.GetName().Name == "Fixie.Tests")
                customDiscoveryTypes = customDiscoveryTypes
                    .Where(type => type.Name == "PrimaryConvention")
                    .ToArray();

            if (customDiscoveryTypes.Length > 1)
            {
                throw new Exception(
                    "A test assembly can have at most one IDiscovery implementation, " +
                    "but the following implementations were discovered:" + Environment.NewLine +
                    string.Join(Environment.NewLine,
                        customDiscoveryTypes
                            .Select(x => $"\t{x.FullName}")));
            }

            return customDiscoveryTypes.SingleOrDefault() ?? typeof(DefaultDiscovery);
        }

        Type ExecutionType()
        {
            var customExecutionTypes = assembly
                .GetTypes()
                .Where(type => IsExecution(type) && !type.IsAbstract)
                .ToArray();

            if (assembly.GetName().Name == "Fixie.Tests")
                customExecutionTypes = customExecutionTypes
                    .Where(type => type.Name == "PrimaryConvention")
                    .ToArray();

            if (customExecutionTypes.Length > 1)
            {
                throw new Exception(
                    "A test assembly can have at most one IExecution implementation, " +
                    "but the following implementations were discovered:" + Environment.NewLine +
                    string.Join(Environment.NewLine,
                        customExecutionTypes
                            .Select(x => $"\t{x.FullName}")));
            }

            return customExecutionTypes.SingleOrDefault() ?? typeof(DefaultExecution);
        }

        static bool IsDiscovery(Type type)
            => type.GetInterfaces().Contains(typeof(IDiscovery));

        static bool IsExecution(Type type)
            => type.GetInterfaces().Contains(typeof(IExecution));

        object Construct(Type type)
        {
            try
            {
                var constructor = type.GetConstructors().Single();
                
                return constructor.Invoke(
                    constructor.GetParameters().Length == 1
                        ? new object?[] { context }
                        : Array.Empty<object>());
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not construct an instance of type '{type.FullName}'.", ex);
            }
        }
    }
}