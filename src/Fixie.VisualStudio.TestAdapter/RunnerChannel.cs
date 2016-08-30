namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Newtonsoft.Json;
    using Runner.Contracts;

    public class RunnerChannel : IDisposable
    {
        readonly IMessageLogger log;
        readonly Socket connectSocket;

        public int Port { get; }

        public RunnerChannel(IMessageLogger log)
        {
            this.log = log;

            const int anyAvailablePort = 0;
            connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connectSocket.Bind(new IPEndPoint(IPAddress.Loopback, anyAvailablePort));
            connectSocket.Listen(10);

            Port = ((IPEndPoint)connectSocket.LocalEndPoint).Port;
        }

        public void HandleMessages(Action<Message, Action<Message>> handle)
        {
            try
            {
                using (var socket = connectSocket.Accept())
                {
                    using (var stream = new NetworkStream(socket))
                    using (var writer = new BinaryWriter(stream))
                    using (var reader = new BinaryReader(stream))
                    {
                        while (true)
                        {
                            var rawMessage = reader.ReadString();

                            var message = JsonConvert.DeserializeObject<Message>(rawMessage);

                            try
                            {
                                handle(message, messageToSend => writer.Write(JsonConvert.SerializeObject(messageToSend)));
                            }
                            catch (Exception exception)
                            {
                                log.Error(exception);
                            }

                            if (message.MessageType == "TestRunner.TestCompleted")
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void Dispose()
        {
            connectSocket.Dispose();
        }
    }
}