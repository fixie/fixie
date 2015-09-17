using Fixie.Execution;
using Fixie.Internal;

namespace Fixie.Tests.Execution
{
    public class RunnerAppDomainCommunicationTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToProvideTheirOwnExecutionSinks()
        {
            typeof(IExecutionSink).ShouldBeSafeAppDomainCommunicationInterface();
        }

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