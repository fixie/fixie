namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO.Pipes;
    using System.Linq;
    using System.Reflection;
    using Cli;
    using Listeners;
    using static System.Console;
    using static Maybe;

    public class EntryPoint
    {
        enum ExitCode
        {
            Success = 0,
            Failure = 1,
            FatalError = -1
        }

        public static int Main(Assembly assembly, string[] arguments)
        {
            try
            {
                CommandLine.Partition(arguments, out var runnerArguments, out var customArguments);

                var options = CommandLine.Parse<Options>(runnerArguments);

                options.Validate();

                var pipeName = Environment.GetEnvironmentVariable("FIXIE_NAMED_PIPE");

                var entryPoint = new EntryPoint();

                if (pipeName == null)
                    return (int)entryPoint.RunAssembly(assembly, options, customArguments);

                using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                {
                    entryPoint.Subscribe(new PipeListener(pipe));

                    pipe.Connect();
                    pipe.ReadMode = PipeTransmissionMode.Message;

                    var exitCode = ExitCode.Success;

                    try
                    {
                        var messageType = pipe.ReceiveMessage();

                        if (messageType == typeof(PipeMessage.DiscoverTests).FullName)
                        {
                            var discoverTests = pipe.Receive<PipeMessage.DiscoverTests>();
                            entryPoint.DiscoverMethods(assembly, customArguments);
                        }
                        else if (messageType == typeof(PipeMessage.ExecuteTests).FullName)
                        {
                            var executeTests = pipe.Receive<PipeMessage.ExecuteTests>();

                            exitCode = executeTests.Filter.Length > 0
                                ? entryPoint.RunTests(assembly, options, customArguments, executeTests.Filter)
                                : entryPoint.RunAssembly(assembly, options, customArguments);
                        }
                        else
                        {
                            var body = pipe.ReceiveMessage();
                            throw new Exception($"Test assembly received unexpected message of type {messageType}: {body}");
                        }
                    }
                    catch (Exception exception)
                    {
                        pipe.Send(exception);
                    }
                    finally
                    {
                        pipe.Send<PipeMessage.Completed>();
                    }

                    return (int)exitCode;
                }
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    WriteLine($"Fatal Error: {exception}");

                return (int)ExitCode.FatalError;
            }
        }

        readonly List<Listener> customListeners = new List<Listener>();

        void Subscribe<TListener>(TListener listener) where TListener : Listener
        {
            customListeners.Add(listener);
        }

        void DiscoverMethods(Assembly assembly, string[] customArguments)
        {
            var listeners = customListeners;
            var bus = new Bus(listeners);
            var discoverer = new Discoverer(bus, customArguments);

            discoverer.DiscoverMethods(assembly);
        }

        ExitCode RunAssembly(Assembly assembly, Options options, string[] customArguments)
        {
            return Run(assembly, options, customArguments, runner => runner.Run());
        }

        ExitCode RunTests(Assembly assembly, Options options, string[] customArguments, PipeMessage.Test[] tests)
        {
            return Run(assembly, options, customArguments,
                r => r.Run(tests.Select(x => new Test(x.Class, x.Method)).ToList()));
        }

        ExitCode Run(Assembly assembly, Options options, string[] customArguments, Func<AssemblyRunner, ExecutionSummary> run)
        {
            var listeners = GetExecutionListeners(options);
            var bus = new Bus(listeners);
            var assemblyRunner = new AssemblyRunner(assembly, bus, customArguments);

            var summary = run(assemblyRunner);

            if (summary.Total == 0)
                return ExitCode.FatalError;

            if (summary.Failed > 0)
                return ExitCode.Failure;

            return ExitCode.Success;
        }

        List<Listener> GetExecutionListeners(Options options)
        {
            return customListeners.Any() ? customListeners : DefaultExecutionListeners(options).ToList();
        }

        static IEnumerable<Listener> DefaultExecutionListeners(Options options)
        {
            if (Try(AzureListener.Create, out var azure))
                yield return azure;

            if (Try(AppVeyorListener.Create, out var appVeyor))
                yield return appVeyor;

            if (Try(() => ReportListener.Create(options), out var report))
                yield return report;

            if (Try(TeamCityListener.Create, out var teamCity))
                yield return teamCity;
            else
                yield return new ConsoleListener();
        }
    }
}