namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using Contracts;
    using Execution;
    using Execution.Listeners;
    using Newtonsoft.Json;
    using Message = Contracts.Message;

    public class DesignTimeRunner : RunnerBase
    {
        public override int Run(string assemblyFullPath, Assembly assembly, Options options, IReadOnlyList<string> conventionArguments)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Loopback, options.Port.Value);
                socket.Connect(ipEndPoint);

                using (var networkStream = new NetworkStream(socket))
                using (var writer = new BinaryWriter(networkStream))
                using (var reader = new BinaryReader(networkStream))
                {
                    var sink = new DesignTimeSink(assemblyFullPath, writer);

                    try
                    {
                        var listeners = new List<Listener>();

                        if (options.List)
                        {
                            listeners.Add(new DesignTimeDiscoveryListener(sink, assemblyFullPath));
                            DiscoverMethods(assembly, conventionArguments, listeners);
                        }
                        else if (options.WaitCommand)
                        {
                            sink.SendWaitingCommand();

                            var rawMessage = reader.ReadString();
                            var message = JsonConvert.DeserializeObject<Message>(rawMessage);
                            var testsToRun = message.Payload.ToObject<RunTestsMessage>().Tests;

                            listeners.Add(new DesignTimeExecutionListener(sink));

                            var summaryListener = new SummaryListener();
                            listeners.Add(summaryListener);

                            if (testsToRun.Any())
                            {
                                var methodGroups = testsToRun;
                                RunMethods(assembly, conventionArguments, methodGroups, listeners);
                            }
                            else
                            {
                                RunAssembly(assembly, conventionArguments, listeners);
                            }

                            return summaryListener.Summary.Failed;
                        }
                    }
                    catch (Exception exception)
                    {
                        sink.Log(exception.ToString());
                        return Program.FatalError;
                    }
                    finally
                    {
                        sink.SendTestCompleted();
                    }
                }
            }

            return 0;
        }
    }
}