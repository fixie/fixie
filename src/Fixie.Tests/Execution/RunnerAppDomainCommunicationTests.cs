using Fixie.Execution;
using Fixie.Internal;

namespace Fixie.Tests.Execution
{
    public class RunnerAppDomainCommunicationTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToProvideTheirOwnHandlersOfCaseResults()
        {
            typeof(IHandler<CaseResult>).ShouldBeSafeAppDomainCommunicationInterface();
        }

        public void ShouldAllowRunnersToPerformTestDiscoveryAndExecutionThroughExecutionProxy()
        {
            typeof(ExecutionProxy).ShouldBeSafeAppDomainCommunicationInterface();
        }
    }
}