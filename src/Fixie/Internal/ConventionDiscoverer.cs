namespace Fixie.Internal
{
    using System;
    using System.Linq;
    using System.Reflection;

    class ConventionDiscoverer
    {
        readonly TestContext context;
        readonly Assembly assembly;

        public ConventionDiscoverer(TestContext context)
        {
            this.context = context;
            assembly = context.Assembly;
        }

        public Convention GetConvention()
        {
            return GetConfiguration().Conventions.Items.Single();
        }

        Configuration GetConfiguration()
        {
            var customConfigurationTypes = assembly
                .GetTypes()
                .Where(type => type.IsSubclassOf(typeof(Configuration)) && !type.IsAbstract)
                .ToArray();

            if (customConfigurationTypes.Length > 1)
            {
                throw new Exception(
                    "A test assembly can have at most one Configuration implementation, " +
                    "but the following implementations were discovered:" + Environment.NewLine +
                    string.Join(Environment.NewLine,
                        customConfigurationTypes
                            .Select(x => $"\t{x.FullName}")));
            }

            var configurationType = customConfigurationTypes.SingleOrDefault();
            
            if (configurationType is null)
                return new DefaultConfiguration();

            return (Configuration) ConstructWithOptionalContext(configurationType);
        }

        object ConstructWithOptionalContext(Type type)
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