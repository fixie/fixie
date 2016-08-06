namespace Fixie.Internal
{
    using System.Collections.Generic;
    using System.Reflection;
    using Execution;

    public class Discoverer
    {
        readonly Bus bus;
        readonly Options options;

        public Discoverer(Bus bus)
            : this(bus, new Options()) { }

        public Discoverer(Bus bus, Options options)
        {
            this.bus = bus;
            this.options = options;
        }

        public void DiscoverMethodGroups(Assembly assembly)
        {
            RunContext.Set(options);

            var conventions = new ConventionDiscoverer(assembly).GetConventions();

            DiscoverMethodGroups(assembly, conventions);
        }

        public void DiscoverMethodGroups(Assembly assembly, Convention convention)
        {
            RunContext.Set(options);

            var conventions = new[] { convention };

            DiscoverMethodGroups(assembly, conventions);
        }

        void DiscoverMethodGroups(Assembly assembly, Convention[] conventions)
        {
            foreach (var convention in conventions)
            {
                var classDiscoverer = new ClassDiscoverer(convention);
                var candidateTypes = assembly.GetTypes();
                var testClasses = classDiscoverer.TestClasses(candidateTypes);

                var methodDiscoverer = new MethodDiscoverer(convention);
                foreach (var testClass in testClasses)
                {
                    var distinctMethodGroups = new Dictionary<string, MethodGroup>();

                    foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                    {
                        var methodGroup = new MethodGroup(testMethod);

                        if (!distinctMethodGroups.ContainsKey(methodGroup.FullName))
                        {
                            distinctMethodGroups[methodGroup.FullName] = methodGroup;
                            bus.Publish(new MethodGroupDiscovered(methodGroup));
                        }
                    }
                }
            }
        }
    }
}