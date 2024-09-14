using Fixie.Internal;
using Fixie.Reports;
using static Fixie.Tests.Utility;

namespace Fixie.Tests.Internal;

public class BusTests
{
    public async Task ShouldPublishEventsToAllReports()
    {
        await using var console = new StringWriter();

        IReport[] reports =
        [
            new EventHandler(console),
            new AnotherEventHandler(console),
            new CombinationEventHandler(console)
        ];

        var bus = new Bus(console, reports);
        await bus.Publish(new Event(1));
        await bus.Publish(new AnotherEvent(2));
        await bus.Publish(new Event(3));

        console.ToString()
            .ShouldBe(
                $"""
                 {FullName<EventHandler>()} handled Event 1
                 {FullName<CombinationEventHandler>()} handled Event 1
                 {FullName<AnotherEventHandler>()} handled AnotherEvent 2
                 {FullName<CombinationEventHandler>()} handled AnotherEvent 2
                 {FullName<EventHandler>()} handled Event 3
                 {FullName<CombinationEventHandler>()} handled Event 3
                 
                 """);
    }

    public async Task ShouldCatchAndLogExceptionsThrowByProblematicReportsRatherThanInterruptExecution()
    {
        await using var console = new StringWriter();
        
        IReport[] reports =
        [
            new EventHandler(console),
            new FailingEventHandler()
        ];

        var bus = new Bus(console, reports);
        await bus.Publish(new Event(1));
        await bus.Publish(new AnotherEvent(2));
        await bus.Publish(new Event(3));

        console.ToString()
            .ShouldBe(
                $"""
                {FullName<EventHandler>()} handled Event 1
                {FullName<FailingEventHandler>()} threw an exception while attempting to handle a message of type {FullName<Event>()}:
                
                {FullName<StubException>()}: Could not handle Event 1
                <<Stack Trace>>
                
                {FullName<EventHandler>()} handled Event 3
                {FullName<FailingEventHandler>()} threw an exception while attempting to handle a message of type {FullName<Event>()}:
                
                {FullName<StubException>()}: Could not handle Event 3
                <<Stack Trace>>
                
                
                """
            );
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

    class EventHandler(StringWriter console) : IHandler<Event>
    {
        public Task Handle(Event message)
        {
            Log<EventHandler, Event>(console, message.Id);
            return Task.CompletedTask;
        }
    }

    class AnotherEventHandler(StringWriter console) : IHandler<AnotherEvent>
    {
        public Task Handle(AnotherEvent message)
        {
            Log<AnotherEventHandler, AnotherEvent>(console, message.Id);
            return Task.CompletedTask;
        }
    }

    class CombinationEventHandler(StringWriter console) : IHandler<Event>, IHandler<AnotherEvent>
    {
        public Task Handle(Event message)
        {
            Log<CombinationEventHandler, Event>(console, message.Id);
            return Task.CompletedTask;
        }

        public Task Handle(AnotherEvent message)
        {
            Log<CombinationEventHandler, AnotherEvent>(console, message.Id);
            return Task.CompletedTask;
        }
    }

    class FailingEventHandler : IHandler<Event>
    {
        public Task Handle(Event message)
            => throw new StubException($"Could not handle {nameof(Event)} {message.Id}");
    }

    static void Log<THandler, TEvent>(StringWriter console, int id)
        => console.WriteLine($"{typeof(THandler).FullName} handled {typeof(TEvent).Name} {id}");

    class StubException : Exception
    {
        public StubException(string message)
            : base(message) { }

        public override string StackTrace
            => "<<Stack Trace>>";
    }
}