using System;
using System.Collections.Generic;
using System.IO;
using Fixie.Discovery;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    [DefaultExecutorUri(VsTestExecutor.Id)]
    [FileExtension(".exe")]
    [FileExtension(".dll")]
    public class VsTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger log, ITestCaseDiscoverySink discoverySink)
        {
            RemotingUtility.CleanUpRegisteredChannels();

            foreach (var source in sources)
            {
                log.Info("Processing " + source);

                try
                {
                    var assemblyFullPath = Path.GetFullPath(source);

                    if (SourceDirectoryContainsFixie(assemblyFullPath))
                    {
                        using (var environment = new ExecutionEnvironment(assemblyFullPath))
                        {
                            var methodGroups = environment.DiscoverTestMethodGroups(new Lookup());

                            foreach (var methodGroup in methodGroups)
                                discoverySink.SendTestCase(new TestCase(methodGroup.FullName, VsTestExecutor.Uri, source));
                        }
                    }
                    else
                    {
                        log.Info("Skipping " + source + " because it is not a test assembly.");
                    }
                }
                catch (Exception exception)
                {
                    log.Error(exception);
                }
            }
        }

        bool SourceDirectoryContainsFixie(string assemblyFileName)
        {
            return File.Exists(Path.Combine(Path.GetDirectoryName(assemblyFileName), "Fixie.dll"));
        }
    }
}
