namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Newtonsoft.Json;
    using Runner.Contracts;

    public class RunnerChannel
    {
        readonly ManualResetEvent terminateWaitHandle;

        public RunnerChannel()
        {
            terminateWaitHandle = new ManualResetEvent(false);
        }

        public void WaitForBackgroundThread()
        {
            terminateWaitHandle.WaitOne();
        }

        public int HandleMessagesOnBackgroundThread(Action<Message, Action<Message>> handle, IMessageLogger log)
        {
            const int anyAvailablePort = 0;

            var connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            connectSocket.Bind(new IPEndPoint(IPAddress.Loopback, anyAvailablePort));
            connectSocket.Listen(10);

            var port = ((IPEndPoint)connectSocket.LocalEndPoint).Port;

            terminateWaitHandle.Reset();

            new Thread(() =>
            {
                try
                {
                    using (connectSocket)
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

                                    log.Info(rawMessage);

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
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }

                log.Info("Exiting background thread.");

                terminateWaitHandle.Set();
            }) { IsBackground = true }.Start();

            return port;
        }
    }
}