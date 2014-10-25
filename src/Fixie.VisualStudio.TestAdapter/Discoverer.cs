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
            logger.SendMessage(TestMessageLevel.Informational, "DiscoverTests invoked with string 'sources' list:");

            foreach (var source in sources)
                logger.SendMessage(TestMessageLevel.Informational, "\tSource: " + source);
        }
    }
}
