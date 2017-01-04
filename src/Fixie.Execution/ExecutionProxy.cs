namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Listeners;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly List<Listener> customListeners = new List<Listener>();

        public void Subscribe<TListener>(object[] listenerArguments) where TListener : Listener
        {
            customListeners.Add((Listener)Activator.CreateInstance(typeof(TListener), listenerArguments));
        }

        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return new Discoverer(options).DiscoverTestMethodGroups(assembly);
        }

        public ExecutionSummary RunAssembly(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Run(options, runner => runner.RunAssembly(assembly));
        }

        public ExecutionSummary RunMethods(string assemblyFullPath, Options options, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Run(options, r => r.RunMethods(assembly, methodGroups));
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        ExecutionSummary Run(Options options, Action<Runner> run)
        {
            var summaryListener = new SummaryListener();

            var listeners = GetListeners(options, summaryListener);
            var bus = new Bus(listeners);
            var runner = new Runner(bus, options);

            run(runner);

            return summaryListener.Summary;
        }

        List<Listener> GetListeners(Options options, SummaryListener summaryListener)
        {
            var listeners = customListeners.Any() ? customListeners : DefaultListeners(options).ToList();

            listeners.Add(summaryListener);

            return listeners;
        }

        static IEnumerable<Listener> DefaultListeners(Options options)
        {
            if (ShouldUseTeamCityListener(options))
                yield return new TeamCityListener();
            else
                yield return new ConsoleListener();

            if (ShouldUseAppVeyorListener())
                yield return new AppVeyorListener();

            foreach (var fileName in options[CommandLineOption.NUnitXml])
                yield return new ReportListener<NUnitXmlReport>(fileName);

            foreach (var fileName in options[CommandLineOption.XUnitXml])
                yield return new ReportListener<XUnitXmlReport>(fileName);
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