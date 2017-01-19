namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Internal;
    using Listeners;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly List<Listener> customListeners = new List<Listener>();

        public void Subscribe<TListener>(object[] listenerArguments) where TListener : Listener
        {
            customListeners.Add((Listener)Activator.CreateInstance(typeof(TListener), listenerArguments));
        }

        public void DiscoverMethods(string assemblyFullPath, string[] arguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var listeners = customListeners;
            var bus = new Bus(listeners);
            var discoverer = new Discoverer(bus, arguments);

            discoverer.DiscoverMethods(assembly);
        }

        public int RunAssembly(string assemblyFullPath, string[] arguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var summary = Run(arguments, runner => runner.RunAssembly(assembly));

            return summary.Failed;
        }

        public void RunMethods(string assemblyFullPath, string[] arguments, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            Run(arguments, r => r.RunMethods(assembly, methodGroups));
        }

        static Options Options(string[] arguments)
        {
            return new CommandLineParser(arguments).Options;
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        ExecutionSummary Run(string[] arguments, Action<Runner> run)
        {
            var summaryListener = new SummaryListener();

            var listeners = GetExecutionListeners(Options(arguments), summaryListener);
            var bus = new Bus(listeners);
            var runner = new Runner(bus, arguments);

            run(runner);

            return summaryListener.Summary;
        }

        List<Listener> GetExecutionListeners(Options options, SummaryListener summaryListener)
        {
            var listeners = customListeners.Any() ? customListeners : DefaultExecutionListeners(options).ToList();

            listeners.Add(summaryListener);

            return listeners;
        }

        static IEnumerable<Listener> DefaultExecutionListeners(Options options)
        {
            if (ShouldUseTeamCityListener(options))
                yield return new TeamCityListener();
            else
                yield return new ConsoleListener();

            if (ShouldUseAppVeyorListener())
                yield return new AppVeyorListener();

            foreach (var format in options[CommandLineOption.ReportFormat])
            {
                if (String.Equals(format, "NUnit", StringComparison.CurrentCultureIgnoreCase))
                    yield return new ReportListener<NUnitXml>();

                else if (String.Equals(format, "xUnit", StringComparison.CurrentCultureIgnoreCase))
                    yield return new ReportListener<XUnitXml>();
            }
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

        static bool ShouldUseAppVeyorListener()
        {
            return Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        }
    }
}