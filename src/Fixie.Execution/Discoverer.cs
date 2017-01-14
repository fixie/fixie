namespace Fixie.Execution
{
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;

    public class Discoverer
    {
        readonly Bus bus;
        readonly Options options;

        public Discoverer(Bus bus, Options options)
        {
            this.bus = bus;
            this.options = options;
        }

        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(Assembly assembly)
        {
            RunContext.Set(options);
            var conventions = new ConventionDiscoverer(assembly).GetConventions();

            var discoveredTestMethodGroups = new List<MethodGroup>();

            foreach (var convention in conventions)
            {
                var classDiscoverer = new ClassDiscoverer(convention);
                var candidateTypes = assembly.GetTypes();
                var testClasses = classDiscoverer.TestClasses(candidateTypes);

                var methodDiscoverer = new MethodDiscoverer(convention);
                foreach (var testClass in testClasses)
                {
                    foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                    {
                        bus.Publish(new MethodDiscovered(testClass, testMethod));

                        discoveredTestMethodGroups.Add(new MethodGroup(testMethod));
                    }
                }
            }

            return discoveredTestMethodGroups;
        }
    }
}