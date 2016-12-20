namespace Fixie.Tests.Execution
{
    using Fixie.Execution;
    using Fixie.Internal;

    public class RunnerAppDomainCommunicationTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToProvideTheirOwnListeners()
        {
            typeof(Listener).ShouldBeSafeAppDomainCommunicationInterface();
        }

        public void ShouldAllowRunnersInOtherAppDomainsToPerformTestDiscoveryAndExecutionThroughExecutionProxy()
        {
            typeof(ExecutionProxy).ShouldBeSafeAppDomainCommunicationInterface();
        }
    }
}