namespace Fixie.Internal
{
    using System.Collections.Generic;
    using System.Reflection;

    public class Discoverer
    {
        readonly Options options;

        public Discoverer(Options options)
        {
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
                    var distinctMethodGroups = new Dictionary<string, MethodGroup>();

                    foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                    {
                        var methodGroup = new MethodGroup(testMethod);

                        distinctMethodGroups[methodGroup.FullName] = methodGroup;
                    }

                    discoveredTestMethodGroups.AddRange(distinctMethodGroups.Values);
                }
            }

            return discoveredTestMethodGroups;
        }
    }
}