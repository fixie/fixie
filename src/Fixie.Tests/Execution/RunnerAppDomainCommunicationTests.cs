using Fixie.Execution;

namespace Fixie.Tests.Execution
{
    public class RunnerAppDomainCommunicationTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToProvideTheirOwnListeners()
        {
            typeof(Listener).ShouldBeSafeAppDomainCommunicationInterface();
        }

        public void ShouldAllowRunnersToPerformTestDiscoveryAndExecutionThroughExecutionProxy()
        {
            typeof(ExecutionProxy).ShouldBeSafeAppDomainCommunicationInterface();
        }
    }
}