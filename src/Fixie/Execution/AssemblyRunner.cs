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

                var executionProxy = new ExecutionProxy(assemblyDirectory);

                var pipeName = Environment.GetEnvironmentVariable("FIXIE_NAMED_PIPE");

                if (pipeName == null)
                    return executionProxy.RunAssembly(assemblyFullPath, arguments);

                using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                {
                    executionProxy.Subscribe(new TestExplorerListener(pipe));

                    pipe.Connect();
                    pipe.ReadMode = PipeTransmissionMode.Message;

                    var command = pipe.ReceiveMessage();

                    int exitCode = Success;

                    if (command == "DiscoverMethods")
                    {
                        executionProxy.DiscoverMethods(assemblyFullPath, arguments);
                    }
                    else if (command == "RunMethods")
                    {
                        var runMethods = pipe.Receive<TestExplorerListener.RunMethods>();

                        exitCode = executionProxy.RunMethods(assemblyFullPath, arguments, runMethods.Methods);
                    }
                    else
                    {
                        exitCode = executionProxy.RunAssembly(assemblyFullPath, arguments);
                    }

                    pipe.SendMessage(typeof(TestExplorerListener.Completed).FullName);

                    return exitCode;
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
}