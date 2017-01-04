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
            var reportListener = ShouldProduceReports(options) ? new ReportListener() : null;

            var listeners = GetListeners(options, summaryListener, reportListener);
            var bus = new Bus(listeners);
            var runner = new Runner(bus, options);

            run(runner);

            if (reportListener != null)
                SaveReport(options, reportListener.Report);

            return summaryListener.Summary;
        }

        List<Listener> GetListeners(Options options, SummaryListener summaryListener, ReportListener reportListener)
        {
            var listeners = customListeners.Any() ? customListeners : GetDefaultListeners(options).ToList();

            if (reportListener != null)
                listeners.Add(reportListener);

            listeners.Add(summaryListener);

            return listeners;
        }

        static IEnumerable<Listener> GetDefaultListeners(Options options)
        {
            if (ShouldUseTeamCityListener(options))
                yield return new TeamCityListener();
            else
                yield return new ConsoleListener();

            if (ShouldUseAppVeyorListener())
                yield return new AppVeyorListener();
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

        static bool ShouldProduceReports(Options options)
        {
            return options.Contains(CommandLineOption.NUnitXml) || options.Contains(CommandLineOption.XUnitXml);
        }

        static void SaveReport(Options options, ExecutionReport executionReport)
        {
            if (options.Contains(CommandLineOption.NUnitXml))
            {
                var xDocument = new NUnitXmlReport().Transform(executionReport);

                foreach (var fileName in options[CommandLineOption.NUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
            }

            if (options.Contains(CommandLineOption.XUnitXml))
            {
                var xDocument = new XUnitXmlReport().Transform(executionReport);

                foreach (var fileName in options[CommandLineOption.XUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
            }
        }
    }
}