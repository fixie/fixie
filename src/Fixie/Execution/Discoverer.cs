namespace Fixie.Execution
{
    using System.Reflection;

    class Discoverer
    {
        readonly Bus bus;
        readonly string[] conventionArguments;

        public Discoverer(Bus bus)
            : this(bus, new string[] {}) { }

        public Discoverer(Bus bus, string[] conventionArguments)
        {
            this.bus = bus;
            this.conventionArguments = conventionArguments;
        }

        public void DiscoverMethods(Assembly assembly)
        {
            var discovery = new ConventionDiscoverer(assembly, conventionArguments).GetDiscovery();

            DiscoverMethods(assembly, discovery);
        }

        public void DiscoverMethods(Assembly assembly, Discovery discovery)
        {
            var classDiscoverer = new ClassDiscoverer(discovery);
            var candidateTypes = assembly.GetTypes();
            var testClasses = classDiscoverer.TestClasses(candidateTypes);

            var methodDiscoverer = new MethodDiscoverer(discovery);
            foreach (var testClass in testClasses)
            foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                bus.Publish(new MethodDiscovered(testMethod));
        }
    }
}