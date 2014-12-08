using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Discovery;

namespace Fixie
{
    public class DiscoveryProxy : MarshalByRefObject
    {
        public IReadOnlyList<TestMethod> TestMethods(string assemblyFullPath)
        {
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
            var runContext = new RunContext(assembly, Enumerable.Empty<string>().ToLookup(x => x, x => x));
            var conventions = new ConventionDiscoverer(runContext).GetConventions();

            var discoveredTestMethods = new List<TestMethod>();

            foreach (var convention in conventions)
            {
                var classDiscoverer = new ClassDiscoverer(convention.Config);
                var candidateTypes = assembly.GetTypes();
                var testClasses = classDiscoverer.TestClasses(candidateTypes);

                var methodDiscoverer = new MethodDiscoverer(convention.Config);
                foreach (var testClass in testClasses)
                {
                    var testMethods = new Dictionary<string, TestMethod>();

                    foreach (var method in methodDiscoverer.TestMethods(testClass))
                    {
                        var testMethod = new TestMethod(method);

                        testMethods[testMethod.MethodGroup] = testMethod;
                    }

                    discoveredTestMethods.AddRange(testMethods.Values);
                }
            }

            return discoveredTestMethods;
        }
    }
}