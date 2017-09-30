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

                    using (var clientPipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                    {
                        clientPipe.ReadMode = PipeTransmissionMode.Message;
                        clientPipe.Connect();

                        var command = clientPipe.ReceiveMessage();

                        if (command == "DiscoverMethods")
                        {
                            executionProxy.DiscoverMethods(assemblyFullPath, arguments);
                            return Success;
                        }

                        if (command == "RunMethods")
                        {
                            var runMethods = clientPipe.Receive<RunMethods>();

                            return executionProxy.RunMethods(assemblyFullPath, arguments, runMethods.Methods);
                        }

                        return executionProxy.RunAssembly(assemblyFullPath, arguments);
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