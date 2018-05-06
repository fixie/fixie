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
            var convention = new ConventionDiscoverer(assembly, conventionArguments).GetConvention();

            DiscoverMethods(assembly, convention);
        }

        public void DiscoverMethods(Assembly assembly, Convention convention)
        {
            var classDiscoverer = new ClassDiscoverer(convention);
            var candidateTypes = assembly.GetTypes();
            var testClasses = classDiscoverer.TestClasses(candidateTypes);

            var methodDiscoverer = new MethodDiscoverer(convention);
            foreach (var testClass in testClasses)
            foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                bus.Publish(new MethodDiscovered(testMethod));
        }
    }
}