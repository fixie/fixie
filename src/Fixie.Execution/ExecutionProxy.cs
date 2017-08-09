namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Cli;
    using Listeners;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly string runnerWorkingDirectory;
        readonly List<Listener> customListeners = new List<Listener>();

        public ExecutionProxy(string runnerWorkingDirectory)
        {
            this.runnerWorkingDirectory = runnerWorkingDirectory;
        }

        public void Subscribe<TListener>(object[] listenerArguments) where TListener : Listener
        {
            customListeners.Add((Listener)Activator.CreateInstance(typeof(TListener), listenerArguments));
        }

        public void DiscoverMethods(string assemblyFullPath, string[] arguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var options = CommandLine.Parse<Options>(arguments, out string[] conventionArguments);

            var listeners = customListeners;
            var bus = new Bus(listeners);
            var discoverer = new Discoverer(bus, conventionArguments);

            discoverer.DiscoverMethods(assembly);
        }

        public int RunAssembly(string assemblyFullPath, string[] arguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var summary = Run(arguments, runner => runner.RunAssembly(assembly));

            return summary.Failed;
        }

        public void RunMethods(string assemblyFullPath, string[] arguments, string[] methods)
        {
            var methodGroups = methods.Select(x => new MethodGroup(x)).ToArray();

            var assembly = LoadAssembly(assemblyFullPath);

            Run(arguments, r => r.RunMethods(assembly, methodGroups));
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(GetAssemblyName(assemblyFullPath));
        }

        static AssemblyName GetAssemblyName(string assemblyFullPath)
        {
#if NET452
            return AssemblyName.GetAssemblyName(assemblyFullPath);
#else
            return new AssemblyName { Name = Path.GetFileNameWithoutExtension(assemblyFullPath) };
#endif
        }

        ExecutionSummary Run(string[] arguments, Func<Runner, ExecutionSummary> run)
        {
            var options = CommandLine.Parse<Options>(arguments, out string[] conventionArguments);

            var listeners = GetExecutionListeners(options);
            var bus = new Bus(listeners);
            var runner = new Runner(bus, conventionArguments);

            return run(runner);
        }

        List<Listener> GetExecutionListeners(Options options)
        {
            return customListeners.Any() ? customListeners : DefaultExecutionListeners(options).ToList();
        }

        IEnumerable<Listener> DefaultExecutionListeners(Options options)
        {
            if (ShouldUseTeamCityListener(options))
                yield return new TeamCityListener();
            else
                yield return new ConsoleListener();

            if (ShouldUseAppVeyorListener())
                yield return new AppVeyorListener();

            if (options.Report != null)
                yield return new ReportListener(SaveReport(options));
        }

        Action<XDocument> SaveReport(Options options)
        {
            return report => ReportListener.Save(report, FullPath(options.Report));
        }

        string FullPath(string absoluteOrRelativePath)
        {
            return Path.Combine(runnerWorkingDirectory, absoluteOrRelativePath);
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
