namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Cli;
    using Listeners;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly List<Listener> customListeners = new List<Listener>();

        public void Subscribe<TListener>(object[] listenerArguments) where TListener : Listener
        {
            customListeners.Add((Listener)Activator.CreateInstance(typeof(TListener), listenerArguments));
        }

        public void DiscoverMethods(string assemblyFullPath, string[] runnerArguments, string[] conventionArguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var listeners = customListeners;
            var bus = new Bus(listeners);
            var discoverer = new Discoverer(bus, conventionArguments);

            discoverer.DiscoverMethods(assembly);
        }

        public int RunAssembly(string assemblyFullPath, string[] runnerArguments, string[] conventionArguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var summary = Run(runnerArguments, conventionArguments, runner => runner.RunAssembly(assembly));

            return summary.Failed;
        }

        public void RunMethods(string assemblyFullPath, string[] runnerArguments, string[] conventionArguments, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            Run(runnerArguments, conventionArguments, r => r.RunMethods(assembly, methodGroups));
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        ExecutionSummary Run(string[] runnerArguments, string[] conventionArguments, Action<Runner> run)
        {
            var summaryListener = new SummaryListener();

            var options = CommandLine.Parse<Options>(runnerArguments);

            var listeners = GetExecutionListeners(options, summaryListener);
            var bus = new Bus(listeners);
            var runner = new Runner(bus, conventionArguments);

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

            if (options.ReportFormat == ReportFormat.NUnit)
                yield return new ReportListener<NUnitXml>();
            else if (options.ReportFormat == ReportFormat.xUnit)
                yield return new ReportListener<XUnitXml>();
        }

        static bool ShouldUseTeamCityListener(Options options)
        {
            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            return options.TeamCity ?? runningUnderTeamCity;
        }

        static bool ShouldUseAppVeyorListener()
        {
            return Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        }
    }
}