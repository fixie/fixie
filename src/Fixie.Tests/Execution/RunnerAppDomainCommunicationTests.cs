namespace Fixie.Tests.Execution
{
    using System.Linq;
    using Fixie.Execution;
    using Fixie.Internal;
    using Fixie.VisualStudio.TestAdapter.Wrappers;
    using Should;

    public class RunnerAppDomainCommunicationTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToProvideTheirOwnMessageHandlers()
        {
            var handlerTypes = typeof(Message)
                .Assembly
                .GetTypes()
                .Where(type => type.IsClass && typeof(Message).IsAssignableFrom(type))
                .Select(messageType => typeof(Handler<>).MakeGenericType(messageType))
                .ToArray();

            handlerTypes.ShouldNotBeEmpty();

            foreach (var handlerType in handlerTypes)
                handlerType.ShouldBeSafeAppDomainCommunicationInterface();
        }

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