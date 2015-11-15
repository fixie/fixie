using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListenerFactory : IListenerFactory
    {
        public Listener Create(Options options)
        {
            return new ConsoleListener();
        }
    }

    public class TeamCityListenerFactory : IListenerFactory
    {
        public Listener Create(Options options)
        {
            return new TeamCityListener();
        }
    }
}