using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListenerFactory : IListenerFactory
    {
        public Listener Create()
        {
            return new ConsoleListener();
        }
    }

    public class TeamCityListenerFactory : IListenerFactory
    {
        public Listener Create()
        {
            return new TeamCityListener();
        }
    }
}