namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Execution;
    using Reports;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly List<Listener> subscribedListeners = new List<Listener>();

        public void Subscribe(string listenerAssemblyFullPath, string listenerType, object[] listenerArgs)
        {
            var listener = Construct<Listener>(listenerAssemblyFullPath, listenerType, listenerArgs);

            subscribedListeners.Add(listener);
        }

        static T Construct<T>(string assemblyFullPath, string typeFullName, object[] args)
        {
            var type = LoadAssembly(assemblyFullPath).GetType(typeFullName);

            return (T)Activator.CreateInstance(type, args);
        }

        public void DiscoverMethodGroups(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var bus = new Bus(subscribedListeners);
            new Discoverer(bus, options).DiscoverMethodGroups(assembly);
        }

        public int RunAssembly(string assemblyFullPath, Options options)
        {
            var summaryListener = new SummaryListener();

            var listeners = Listeners(options).ToList();

            listeners.Add(summaryListener);

            RunAssembly(assemblyFullPath, options, listeners);

            return summaryListener.Summary.Failed;
        }

        public void RunAssembly(string assemblyFullPath, Options options, IReadOnlyList<Listener> listeners)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var bus = new Bus(listeners);
            Runner(options, bus).RunAssembly(assembly);
        }

        public void RunMethods(string assemblyFullPath, Options options, Listener listener, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var bus = new Bus(listener);
            Runner(options, bus).RunMethods(assembly, methodGroups);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        static Runner Runner(Options options, Bus bus)
        {
            return new Runner(bus, options);
        }

        static IEnumerable<Listener> Listeners(Options options)
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