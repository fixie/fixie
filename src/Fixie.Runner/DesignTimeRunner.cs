namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Contracts;
    using Execution;
    using Newtonsoft.Json;
    using Message = Contracts.Message;

    public class DesignTimeRunner
    {
        public static int Run(Options options, IReadOnlyList<string> conventionArguments)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Loopback, options.Port.Value);
                socket.Connect(ipEndPoint);

                using (var networkStream = new NetworkStream(socket))
                using (var writer = new BinaryWriter(networkStream))
                using (var reader = new BinaryReader(networkStream))
                using (var sink = new DesignTimeSink(writer))
                {
                    try
                    {
                        if (options.List)
                        {
                            DiscoverTests(sink, options.AssemblyPath, conventionArguments);
                        }
                        else if (options.WaitCommand)
                        {
                            sink.SendWaitingCommand();

                            var rawMessage = reader.ReadString();
                            var message = JsonConvert.DeserializeObject<Message>(rawMessage);
                            var testsToRun = message.Payload.ToObject<RunTestsMessage>().Tests;

                            if (testsToRun.Any())
                                RunTests(sink, options.AssemblyPath, conventionArguments, testsToRun);
                            else
                                RunAllTests(sink, options.AssemblyPath, conventionArguments);
                        }

                        sink.SendTestCompleted();
                    }
                    catch (Exception exception)
                    {
                        sink.Log(exception.ToString());
                    }
                }
            }

            return 0;
        }

        static void DiscoverTests(DesignTimeSink sink, string assemblyPath, IReadOnlyList<string> conventionArguments)
        {
            using (var environment = new ExecutionEnvironment(assemblyPath))
            {
                environment.Subscribe<DesignTimeDiscoveryListener>(sink, assemblyPath);
                environment.DiscoverMethodGroups(conventionArguments.ToArray());
            }
        }

        static void RunAllTests(DesignTimeSink sink, string assemblyPath, IReadOnlyList<string> conventionArguments)
        {
            using (var environment = new ExecutionEnvironment(assemblyPath))
            {
                environment.Subscribe<DesignTimeExecutionListener>(sink);
                environment.RunAssembly(conventionArguments.ToArray());
            }
        }

        static void RunTests(DesignTimeSink sink, string assemblyPath, IReadOnlyList<string> conventionArguments, IReadOnlyList<string> testsToRun)
        {
            using (var environment = new ExecutionEnvironment(assemblyPath))
            {
                environment.Subscribe<DesignTimeExecutionListener>(sink);
                var methodGroups = testsToRun.Select(x => new MethodGroup(x)).ToArray();
                environment.RunMethods(methodGroups, conventionArguments.ToArray());
            }
        }
    }
}