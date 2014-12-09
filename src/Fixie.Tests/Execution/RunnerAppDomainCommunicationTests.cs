using Fixie.Execution;

namespace Fixie.Tests.Execution
{
    public class RunnerAppDomainCommunicationTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToProvideTheirOwnListeners()
        {
            typeof(Listener).ShouldBeSafeAppDomainCommunicationInterface();
        }

        public void ShouldAllowRunnersToPerformTestDiscoveryThroughDiscoveryProxy()
        {
            typeof(DiscoveryProxy).ShouldBeSafeAppDomainCommunicationInterface();
        }

        public void ShouldAllowRunnersToPerformTestExecutionThroughExecutionProxy()
        {
            typeof(ExecutionProxy).ShouldBeSafeAppDomainCommunicationInterface();
        }
    }
}