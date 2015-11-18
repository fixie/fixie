using System;
using Fixie.Execution;
using Fixie.Internal;

namespace Fixie.Tests.Execution
{
    public class BusTests
    {
        readonly Bus bus;

        public BusTests()
        {
            bus = new Bus();
            bus.Subscribe(new EventHandler());
            bus.Subscribe(new AnotherEventHandler());
            bus.Subscribe(new CombinationEventHandler());
        }

        public void ShouldPublishEventsForAllSubscribers()
        {
            using (var console = new RedirectedConsole())
            {
                bus.Publish(new Event(1));
                bus.Publish(new AnotherEvent(2));
                bus.Publish(new Event(3));

                console.Output.Lines()
                    .ShouldEqual(
                        "EventHandler handled Event 1",
                        "CombinationEventHandler handled Event 1",
                        "AnotherEventHandler handled AnotherEvent 2",
                        "CombinationEventHandler handled AnotherEvent 2",
                        "EventHandler handled Event 3",
                        "CombinationEventHandler handled Event 3");
            }
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
            public void Handle(Event message)
                => Log<EventHandler, Event>(message.Id);
        }

        class AnotherEventHandler : IHandler<AnotherEvent>
        {
            public void Handle(AnotherEvent message)
                => Log<AnotherEventHandler, AnotherEvent>(message.Id);
        }

        class CombinationEventHandler : IHandler<Event>, IHandler<AnotherEvent>
        {
            public void Handle(Event message)
                => Log<CombinationEventHandler, Event>(message.Id);

            public void Handle(AnotherEvent message)
                => Log<CombinationEventHandler, AnotherEvent>(message.Id);
        }

        static void Log<THandler, TEvent>(int id)
            => Console.WriteLine($"{typeof(THandler).Name} handled {typeof(TEvent).Name} {id}");
    }
}