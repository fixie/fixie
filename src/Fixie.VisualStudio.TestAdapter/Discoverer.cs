using System;
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
                            var discovery = environment.Create<DiscoveryProxy>();

                            foreach (var methodGroup in discovery.TestMethodGroups(assemblyFullPath, new Lookup()))
                                discoverySink.SendTestCase(new TestCase(methodGroup.FullName, Executor.Uri, source));
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
