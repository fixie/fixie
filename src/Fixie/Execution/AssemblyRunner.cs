namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Cli;
    using Listeners;

    public class AssemblyRunner
    {
        const int Success = 0;
        const int FatalError = -1;

        public static int Main(string[] arguments)
        {
            try
            {
                CommandLine.Partition(arguments, out var runnerArguments, out var conventionArguments);

                var options = CommandLine.Parse<Options>(runnerArguments);

                options.Validate();

                var assembly = Assembly.GetEntryAssembly();

                var pipeName = Environment.GetEnvironmentVariable("FIXIE_NAMED_PIPE");

                var runner = new AssemblyRunner();

                if (pipeName == null)
                    return runner.RunAssembly(assembly, options, conventionArguments);

                using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                {
                    runner.Subscribe(new PipeListener(pipe));

                    pipe.Connect();
                    pipe.ReadMode = PipeTransmissionMode.Message;

                    int exitCode = Success;

                    try
                    {
                        var messageType = pipe.ReceiveMessage();

                        if (messageType == typeof(PipeMessage.DiscoverTests).FullName)
                        {
                            var discoverTests = pipe.Receive<PipeMessage.DiscoverTests>();
                            runner.DiscoverMethods(assembly, conventionArguments);
                        }
                        else if (messageType == typeof(PipeMessage.ExecuteTests).FullName)
                        {
                            var executeTests = pipe.Receive<PipeMessage.ExecuteTests>();

                            exitCode = executeTests.Filter.Length > 0
                                ? runner.RunTests(assembly, options, conventionArguments, executeTests.Filter)
                                : runner.RunAssembly(assembly, options, conventionArguments);
                        }
                        else
                        {
                            var body = pipe.ReceiveMessage();
                            throw new Exception($"Test assembly received unexpected message of type {messageType}: {body}");
                        }
                    }
                    catch (PreservedException exception)
                    {
                        pipe.Send(exception.OriginalException);
                    }
                    catch (Exception exception)
                    {
                        pipe.Send(exception);
                    }
                    finally
                    {
                        pipe.Send<PipeMessage.Completed>();
                    }

                    return exitCode;
                }
            }
            catch (PreservedException exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception.OriginalException}");

                return FatalError;
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");

                return FatalError;
            }
        }

        readonly List<Listener> customListeners = new List<Listener>();

        void Subscribe<TListener>(TListener listener) where TListener : Listener
        {
            customListeners.Add(listener);
        }

        void DiscoverMethods(Assembly assembly, string[] conventionArguments)
        {
            var listeners = customListeners;
            var bus = new Bus(listeners);
            var discoverer = new Discoverer(bus, conventionArguments);

            discoverer.DiscoverMethods(assembly);
        }

        int RunAssembly(Assembly assembly, Options options, string[] conventionArguments)
        {
            return Run(options, conventionArguments, runner => runner.RunAssembly(assembly));
        }

        int RunTests(Assembly assembly, Options options, string[] conventionArguments, PipeMessage.Test[] tests)
        {
            return Run(options, conventionArguments,
                r => r.RunTests(assembly, tests.Select(x => new Test(x.Class, x.Method)).ToArray()));
        }

        int Run(Options options, string[] conventionArguments, Func<Runner, ExecutionSummary> run)
        {
            var listeners = GetExecutionListeners(options);
            var bus = new Bus(listeners);
            var runner = new Runner(bus, conventionArguments);

            var summary = run(runner);

            return summary.Total == 0 ? FatalError : summary.Failed;
        }

        List<Listener> GetExecutionListeners(Options options)
        {
            return customListeners.Any() ? customListeners : DefaultExecutionListeners(options).ToList();
        }

        static IEnumerable<Listener> DefaultExecutionListeners(Options options)
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

        static Action<XDocument> SaveReport(Options options)
        {
            return report => ReportListener.Save(report, FullPath(options.Report));
        }

        static string FullPath(string absoluteOrRelativePath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), absoluteOrRelativePath);
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