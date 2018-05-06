namespace Fixie.Internal
{
    using System.Reflection;

    class Discoverer
    {
        readonly Bus bus;
        readonly string[] customArguments;

        public Discoverer(Bus bus)
            : this(bus, new string[] {}) { }

        public Discoverer(Bus bus, string[] customArguments)
        {
            this.bus = bus;
            this.customArguments = customArguments;
        }

        public void DiscoverMethods(Assembly assembly)
        {
            var discovery = new BehaviorDiscoverer(assembly, customArguments).GetDiscovery();

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