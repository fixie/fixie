using Fixie.ConsoleRunner;
using Fixie.Tests.Execution;

namespace Fixie.Tests.ConsoleRunner
{
    public class ReportListenerTests
    {
        public void ShouldSupportReceivingMessagesFromTheChildAppDomain()
        {
            typeof(ReportListener).ShouldSupportReceivingMessagesFromTheChildAppDomain();
        }
    }
}