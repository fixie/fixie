using System.Linq;
using Fixie.Execution;
using Fixie.Internal;
using Should;

namespace Fixie.Tests.Execution
{
    public class RunnerAppDomainCommunicationTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToProvideTheirOwnMessageHandlers()
        {
            var handlerTypes = typeof(IMessage)
                .Assembly
                .GetTypes()
                .Where(type => type.IsClass && typeof(IMessage).IsAssignableFrom(type))
                .Select(messageType => typeof(IHandler<>).MakeGenericType(messageType))
                .ToArray();

            handlerTypes.ShouldNotBeEmpty();

            foreach (var handlerType in handlerTypes)
                handlerType.ShouldBeSafeAppDomainCommunicationInterface();
        }

        public void ShouldAllowRunnersInOtherAppDomainsToPerformTestDiscoveryAndExecutionThroughExecutionProxy()
        {
            typeof(ExecutionProxy).ShouldBeSafeAppDomainCommunicationInterface();
        }
    }
}