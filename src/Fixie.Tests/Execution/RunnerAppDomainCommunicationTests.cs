namespace Fixie.Tests.Execution
{
    using Fixie.Runner;

    public class RunnerAppDomainCommunicationTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToPerformTestDiscoveryAndExecutionThroughExecutionProxy()
        {
            typeof(ExecutionProxy).ShouldBeSafeAppDomainCommunicationInterface();
        }

        public void ShouldAllowRunnersInOtherAppDomainsToReportTestDiscoveryAndExecutionToVisualStudio()
        {
            typeof(DesignTimeSink).ShouldBeSafeAppDomainCommunicationInterface();
        }
    }
}