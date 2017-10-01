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

                var assembly = Assembly.GetEntryAssembly();
                var assemblyFullPath = assembly.Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
                var executionProxy = new ExecutionProxy(assemblyDirectory);

                var pipeName = Environment.GetEnvironmentVariable("FIXIE_NAMED_PIPE");

                if (pipeName == null)
                    return executionProxy.RunAssembly(assembly, arguments);

                using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                {
                    executionProxy.Subscribe(new PipeListener(pipe));

                    pipe.Connect();
                    pipe.ReadMode = PipeTransmissionMode.Message;

                    var command = (PipeCommand)Enum.Parse(typeof(PipeCommand), pipe.ReceiveMessage());

                    int exitCode = Success;

                    switch (command)
                    {
                        case PipeCommand.DiscoverMethods:
                            executionProxy.DiscoverMethods(assembly, arguments);
                            break;
                        case PipeCommand.RunMethods:
                            var runMethods = pipe.Receive<PipeListener.RunMethods>();
                            exitCode = executionProxy.RunMethods(assembly, arguments, runMethods.Methods);
                            break;
                        case PipeCommand.RunAssembly:
                            exitCode = executionProxy.RunAssembly(assembly, arguments);
                            break;
                    }

                    pipe.SendMessage(typeof(PipeListener.Completed).FullName);

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