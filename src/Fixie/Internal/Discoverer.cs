namespace Fixie.Internal
{
    using System.Reflection;

    class Discoverer
    {
        readonly Assembly assembly;
        readonly Bus bus;
        readonly string[] customArguments;

        public Discoverer(Assembly assembly, Bus bus)
            : this(assembly, bus, new string[] {}) { }

        public Discoverer(Assembly assembly, Bus bus, string[] customArguments)
        {
            this.assembly = assembly;
            this.bus = bus;
            this.customArguments = customArguments;
        }

        public void DiscoverMethods()
        {
            var discovery = new BehaviorDiscoverer(assembly, customArguments).GetDiscovery();

            try
            {
                var classDiscoverer = new ClassDiscoverer(discovery);
                var candidateTypes = assembly.GetTypes();
                var testClasses = classDiscoverer.TestClasses(candidateTypes);

                var methodDiscoverer = new MethodDiscoverer(discovery);
                foreach (var testClass in testClasses)
                foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                    bus.Publish(new MethodDiscovered(testMethod));
            }
            finally
            {
                discovery.Dispose();
            }
        }
    }
}