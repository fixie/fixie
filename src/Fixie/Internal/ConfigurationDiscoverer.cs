namespace Fixie.Internal
{
    using System;
    using System.Linq;
    using System.Reflection;

    class ConfigurationDiscoverer
    {
        readonly TestContext context;
        readonly Assembly assembly;

        public ConfigurationDiscoverer(TestContext context)
        {
            this.context = context;
            assembly = context.Assembly;
        }

        public Configuration GetConfiguration()
        {
            var customTestProjectTypes = assembly
                .GetTypes()
                .Where(type => IsTestProject(type) && !type.IsAbstract)
                .ToArray();

            if (customTestProjectTypes.Length > 1)
            {
                throw new Exception(
                    "A test assembly can have at most one ITestProject implementation, " +
                    "but the following implementations were discovered:" + Environment.NewLine +
                    string.Join(Environment.NewLine,
                        customTestProjectTypes
                            .Select(x => $"\t{x.FullName}")));
            }

            var configuration = new Configuration();
            
            var testProjectType = customTestProjectTypes.SingleOrDefault();
            
            if (testProjectType != null)
            {
                var testProject = (ITestProject) Construct(testProjectType);

                testProject.Configure(configuration, context);
            }

            if (configuration.Conventions.Items.Count == 0)
                configuration.Conventions.Add<DefaultDiscovery, DefaultExecution>();

            return configuration;
        }

        static bool IsTestProject(Type type)
            => type.GetInterfaces().Contains(typeof(ITestProject));

        static object Construct(Type type)
        {
            try
            {
                return type.GetConstructors().Single().Invoke(null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not construct an instance of type '{type.FullName}'.", ex);
            }
        }
    }
}