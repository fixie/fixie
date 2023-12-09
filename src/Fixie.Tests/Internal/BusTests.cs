namespace Fixie.Tests.Internal;

using System;
using System.Threading.Tasks;
using Assertions;
using Fixie.Internal;
using Fixie.Reports;
using static Utility;

public class BusTests
{
    public async Task ShouldPublishEventsToAllReports()
    {
            var reports = new IReport[]
            {
                new EventHandler(),
                new AnotherEventHandler(),
                new CombinationEventHandler()
            };

            using var console = new RedirectedConsole();

            var bus = new Bus(Console.Out, reports);
            await bus.Publish(new Event(1));
            await bus.Publish(new AnotherEvent(2));
            await bus.Publish(new Event(3));

            console.Lines()
                .ShouldBe(
                    FullName<EventHandler>() + " handled Event 1",
                    FullName<CombinationEventHandler>() + " handled Event 1",
                    FullName<AnotherEventHandler>() + " handled AnotherEvent 2",
                    FullName<CombinationEventHandler>() + " handled AnotherEvent 2",
                    FullName<EventHandler>() + " handled Event 3",
                    FullName<CombinationEventHandler>() + " handled Event 3");
        }

    public async Task ShouldCatchAndLogExceptionsThrowByProblematicReportsRatherThanInterruptExecution()
    {
            var reports = new IReport[]
            {
                new EventHandler(),
                new FailingEventHandler()
            };

            using var console = new RedirectedConsole();

            var bus = new Bus(Console.Out, reports);
            await bus.Publish(new Event(1));
            await bus.Publish(new AnotherEvent(2));
            await bus.Publish(new Event(3));

            console.Lines()
                .ShouldBe(
                    FullName<EventHandler>() + " handled Event 1",
                    FullName<FailingEventHandler>() + $" threw an exception while attempting to handle a message of type {FullName<Event>()}:",
                    "",
                    FullName<StubException>() + ": Could not handle Event 1",
                    "<<Stack Trace>>",
                    "",
                    FullName<EventHandler>() + " handled Event 3",
                    FullName<FailingEventHandler>() + $" threw an exception while attempting to handle a message of type {FullName<Event>()}:",
                    "",
                    FullName<StubException>() + ": Could not handle Event 3",
                    "<<Stack Trace>>");
        }

    class Event : IMessage
    {
        public Event(int id) { Id = id; }
        public int Id { get; }
    }

    class AnotherEvent : IMessage
    {
        public AnotherEvent(int id) { Id = id; }
        public int Id { get; }
    }

    class EventHandler : IHandler<Event>
    {
        public Task Handle(Event message)
        {
                Log<EventHandler, Event>(message.Id);
                return Task.CompletedTask;
            }
    }

    class AnotherEventHandler : IHandler<AnotherEvent>
    {
        public Task Handle(AnotherEvent message)
        {
                Log<AnotherEventHandler, AnotherEvent>(message.Id);
                return Task.CompletedTask;
            }
    }

    class CombinationEventHandler : IHandler<Event>, IHandler<AnotherEvent>
    {
        public Task Handle(Event message)
        {
                Log<CombinationEventHandler, Event>(message.Id);
                return Task.CompletedTask;
            }

        public Task Handle(AnotherEvent message)
        {
                Log<CombinationEventHandler, AnotherEvent>(message.Id);
                return Task.CompletedTask;
            }
    }

    class FailingEventHandler : IHandler<Event>
    {
        public Task Handle(Event message)
            => throw new StubException($"Could not handle {nameof(Event)} {message.Id}");
    }

    static void Log<THandler, TEvent>(int id)
        => Console.WriteLine($"{typeof(THandler).FullName} handled {typeof(TEvent).Name} {id}");

    class StubException : Exception
    {
        public StubException(string message)
            : base(message) { }

        public override string StackTrace
            => "<<Stack Trace>>";
    }
}