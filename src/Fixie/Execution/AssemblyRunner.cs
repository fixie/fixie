namespace Fixie.Execution
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Reflection;
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
                var options = CommandLine.Parse<Options>(arguments);
                options.Validate();

                var assemblyFullPath = Assembly.GetEntryAssembly().Location;

                var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);

                using (var executionProxy = new ExecutionProxy(assemblyDirectory))
                {
                    var pipeName = Environment.GetEnvironmentVariable("FIXIE_NAMED_PIPE");

                    if (pipeName == null)
                        return executionProxy.RunAssembly(assemblyFullPath, arguments);

                    using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                    {
                        executionProxy.Subscribe(new TestExplorerListener(pipe));

                        pipe.Connect();
                        pipe.ReadMode = PipeTransmissionMode.Message;

                        var command = pipe.ReceiveMessage();

                        if (command == "DiscoverMethods")
                        {
                            executionProxy.DiscoverMethods(assemblyFullPath, arguments);

                            pipe.SendMessage(typeof(TestExplorerListener.Completed).FullName);
                            pipe.Send(new TestExplorerListener.Completed());

                            return Success;
                        }
                        else if (command == "RunMethods")
                        {
                            var runMethods = pipe.Receive<RunMethods>();

                            var failures = executionProxy.RunMethods(assemblyFullPath, arguments, runMethods.Methods);

                            pipe.SendMessage(typeof(TestExplorerListener.Completed).FullName);
                            pipe.Send(new TestExplorerListener.Completed());

                            return failures;
                        }
                        else
                        {
                            var failures = executionProxy.RunAssembly(assemblyFullPath, arguments);

                            pipe.SendMessage(typeof(TestExplorerListener.Completed).FullName);
                            pipe.Send(new TestExplorerListener.Completed());

                            return failures;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");

                return FatalError;
            }
        }
    }

    public class RunMethods
    {
        public string[] Methods { get; set; }
    }
}