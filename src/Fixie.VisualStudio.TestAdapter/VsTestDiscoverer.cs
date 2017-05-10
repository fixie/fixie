using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Fixie.Execution;
using System.Linq;

namespace Fixie.VisualStudio.TestAdapter
{
    [DefaultExecutorUri(VsTestExecutor.Id)]
    [FileExtension(".exe")]
    [FileExtension(".dll")]
    public class VsTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger log, ITestCaseDiscoverySink discoverySink)
        {
            log.Version();

            RemotingUtility.CleanUpRegisteredChannels();

            foreach (var assemblyPath in sources)
            {
                try
                {
                    if (AssemblyDirectoryContainsFixie(assemblyPath))
                    {
                        log.Info("Processing " + assemblyPath);

                        var sourceLocationProvider = new SourceLocationProvider(assemblyPath);

                        using (var environment = new ExecutionEnvironment(assemblyPath))
                        {
                            var methodGroups = environment.DiscoverTestMethodGroups(new Options());

                            var testCases = methodGroups.GroupBy(mg => mg).SelectMany(g =>
                            {
                                var fewMethodsWithSameName = g.Count() > 1;
                                return g.Select(tc =>
                                {
                                    var testCase = new TestCase(fewMethodsWithSameName ? tc.FullName : tc.Method, VsTestExecutor.Uri, assemblyPath);
                                    SourceLocation sourceLocation;
                                    try
                                    {
                                        if (sourceLocationProvider.TryGetSourceLocation(tc, out sourceLocation))
                                        {
                                            testCase.CodeFilePath = sourceLocation.CodeFilePath;
                                            testCase.LineNumber = sourceLocation.LineNumber;
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        log.Error(exception);
                                    }
                                    return testCase;
                                });
                            });
                            
                            foreach (var testCase in testCases)
                                discoverySink.SendTestCase(testCase);
                        }
                    }
                    else
                    {
                        log.Info("Skipping " + assemblyPath + " because it is not a test assembly.");
                    }
                }
                catch (Exception exception)
                {
                    log.Error(exception);
                }
            }
        }

        static bool AssemblyDirectoryContainsFixie(string assemblyPath)
        {
            return File.Exists(Path.Combine(Path.GetDirectoryName(assemblyPath), "Fixie.dll"));
        }
    }
}
