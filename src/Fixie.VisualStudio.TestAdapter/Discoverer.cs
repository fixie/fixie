using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.VisualStudio.TestAdapter
{
    [DefaultExecutorUri(Executor.Id)]
    [FileExtension(".exe")]
    [FileExtension(".dll")]
    public class Discoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            foreach (var assemblyPath in sources)
            {
                discoverySink.SendTestCase(ToVsTestCase("Pass", assemblyPath));
                discoverySink.SendTestCase(ToVsTestCase("Fail", assemblyPath));
                discoverySink.SendTestCase(ToVsTestCase("Skip", assemblyPath));
            }
        }

        static TestCase ToVsTestCase(string name, string assemblyPath)
        {
            var fullyQualifiedName = "Test Case " + name;

            return new TestCase(fullyQualifiedName, Executor.Uri, assemblyPath)
            {
                DisplayName = name
            };
        }
    }
}
