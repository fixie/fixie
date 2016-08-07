namespace Fixie.Tests.Execution
{
    using Fixie.Internal;
    using Fixie.VisualStudio.TestAdapter.Wrappers;

    public class RunnerAppDomainCommunicationTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToPerformTestDiscoveryAndExecutionThroughExecutionProxy()
        {
            typeof(ExecutionProxy).ShouldBeSafeAppDomainCommunicationInterface();
        }

        public void ShouldAllowRunnersInOtherAppDomainsToReportTestDiscoveryAndExecutionToVisualStudio()
        {
            typeof(MessageLogger).ShouldBeSafeAppDomainCommunicationInterface();
            typeof(TestCaseDiscoverySink).ShouldBeSafeAppDomainCommunicationInterface();
            typeof(TestExecutionRecorder).ShouldBeSafeAppDomainCommunicationInterface();
        }
    }
}