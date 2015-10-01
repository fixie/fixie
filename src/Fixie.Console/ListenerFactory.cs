using System;
using System.Linq;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ExecutionSink : LongLivedMarshalByRefObject, IExecutionSink
    {
        public void SendMessage(string message)
        {
            Console.WriteLine("SENDMESSAGE: " + message);
        }

        public void RecordResult(CaseResult caseResult)
        {
            Console.WriteLine("RECORDRESULT: " + caseResult.Name);
        }
    }

    public class ListenerFactory : IListenerFactory
    {
        public ListenerFactory(IExecutionSink executionSink, string message)
        {
            executionSink.SendMessage("Message sent to execution sink from within the ListenerFactory's own constructor: " + message);
        }

        public Listener Create(Options options)
        {
            if (ShouldUseTeamCityListener(options))
                return new TeamCityListener();

            return new ConsoleListener();
        }

        static bool ShouldUseTeamCityListener(Options options)
        {
            var teamCityExplicitlySpecified = options.Contains(CommandLineOption.TeamCity);

            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            var useTeamCityListener =
                (teamCityExplicitlySpecified && options[CommandLineOption.TeamCity].First() == "on") ||
                (!teamCityExplicitlySpecified && runningUnderTeamCity);

            return useTeamCityListener;
        }
    }
}