namespace Fixie.Execution
{
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;

    public class Discoverer
    {
        readonly Bus bus;
        readonly string[] conventionArguments;

        public Discoverer(Bus bus, params string[] conventionArguments)
        {
            this.bus = bus;
            this.conventionArguments = conventionArguments;
        }

        public void DiscoverMethodGroups(Assembly assembly)
        {
            RunContext.Set(conventionArguments);

            var conventions = new ConventionDiscoverer(assembly).GetConventions();

            DiscoverMethodGroups(assembly, conventions);
        }

        public void DiscoverMethodGroups(Assembly assembly, Convention convention)
        {
            RunContext.Set(conventionArguments);

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
                    foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                        bus.Publish(new MethodDiscovered(testClass, testMethod));
            }
        }
    }
}