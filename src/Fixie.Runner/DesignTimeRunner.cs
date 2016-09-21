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
                            string assemblyPath = options.AssemblyPath;
                            using (var environment = new ExecutionEnvironment(assemblyPath))
                                DiscoverTests(sink, assemblyPath, conventionArguments, environment);
                        }
                        else if (options.WaitCommand)
                        {
                            sink.SendWaitingCommand();

                            var rawMessage = reader.ReadString();
                            var message = JsonConvert.DeserializeObject<Message>(rawMessage);
                            var testsToRun = message.Payload.ToObject<RunTestsMessage>().Tests;

                            if (testsToRun.Any())
                            {
                                using (var environment = new ExecutionEnvironment(options.AssemblyPath))
                                    RunTests(sink, conventionArguments, testsToRun, environment);
                            }
                            else
                            {
                                using (var environment = new ExecutionEnvironment(options.AssemblyPath))
                                    RunAllTests(sink, conventionArguments, environment);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        sink.Log(exception.ToString());
                    }
                    finally
                    {
                        sink.SendTestCompleted();
                    }
                }
            }

            return 0;
        }

        static void DiscoverTests(DesignTimeSink sink, string assemblyPath, IReadOnlyList<string> conventionArguments, ExecutionEnvironment environment)
        {
            environment.Subscribe<DesignTimeDiscoveryListener>(sink, assemblyPath);
            environment.DiscoverMethodGroups(conventionArguments);
        }

        static void RunAllTests(DesignTimeSink sink, IReadOnlyList<string> conventionArguments, ExecutionEnvironment environment)
        {
            environment.Subscribe<DesignTimeExecutionListener>(sink);
            environment.RunAssembly(conventionArguments);
        }

        static void RunTests(DesignTimeSink sink, IReadOnlyList<string> conventionArguments, IReadOnlyList<string> testsToRun, ExecutionEnvironment environment)
        {
            environment.Subscribe<DesignTimeExecutionListener>(sink);
            var methodGroups = testsToRun;
            environment.RunMethods(methodGroups, conventionArguments);
        }
    }
}