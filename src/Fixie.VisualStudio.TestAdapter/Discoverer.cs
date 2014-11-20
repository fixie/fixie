using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    [DefaultExecutorUri(Executor.Id)]
    [FileExtension(".exe")]
    [FileExtension(".dll")]
    public class Discoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            RemotingUtility.CleanUpRegisteredChannels();

            foreach (var source in sources)
            {
                var assemblyFullPath = Path.GetFullPath(source);

                using (var environment = new ExecutionEnvironment(assemblyFullPath))
                {
                    var discovery = environment.Create<DiscoveryProxy>();

                    foreach (var testMethod in discovery.TestMethods(assemblyFullPath))
                    {
                        discoverySink.SendTestCase(new TestCase(testMethod.FullName, Executor.Uri, source)
                        {
                            DisplayName = testMethod.FullName
                        });
                    }
                }
            }
        }
    }
}
