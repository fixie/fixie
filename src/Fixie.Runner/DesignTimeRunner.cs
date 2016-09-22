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

    public class DesignTimeRunner : RunnerBase
    {
        public override int Run(string assemblyFullPath, Options options, IReadOnlyList<string> conventionArguments)
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
                        var listeners = new List<Listener>();

                        if (options.List)
                        {
                            listeners.Add(new DesignTimeDiscoveryListener(sink, assemblyFullPath));
                            DiscoverMethodGroups(listeners, assemblyFullPath, conventionArguments);
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
                                RunMethods(listeners, assemblyFullPath, methodGroups, conventionArguments);
                            }
                            else
                            {
                                RunAssembly(listeners, assemblyFullPath, conventionArguments);
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