namespace Fixie.Tests.Execution
{
    using System.Linq;
    using Fixie.Execution;
    using Fixie.Internal;
    using Should;

    public class RunnerAppDomainCommunicationTests
    {
        public void AllMessageTypesShouldBeSafeToPassAcrossAppDomains()
        {
            var knownMessageTypes = typeof(Message)
                .Assembly
                .GetTypes()
                .Where(type => typeof(Message).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

            foreach (var messageType in knownMessageTypes)
                messageType.IsSafeForAppDomainCommunication().ShouldBeTrue();
        }

        public void ShouldAllowRunnersInOtherAppDomainsToPerformTestDiscoveryAndExecutionThroughExecutionProxy()
        {
            typeof(ExecutionProxy).ShouldBeSafeAppDomainCommunicationInterface();
        }
    }
}