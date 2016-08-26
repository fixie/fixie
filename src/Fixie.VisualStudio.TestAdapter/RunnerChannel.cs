namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Newtonsoft.Json;
    using Runner.Contracts;

    public class RunnerChannel : IDisposable
    {
        readonly MessageQueue messages;
        BinaryWriter writer;
        BinaryReader reader;

        public Socket ConnectSocket { get; }
        public int Port { get; }

        public static RunnerChannel CreateAndListen(MessageQueue messages)
        {
            const int anyAvailablePort = 0;

            var listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, anyAvailablePort));
            listenSocket.Listen(10);

            return new RunnerChannel(listenSocket, ((IPEndPoint)listenSocket.LocalEndPoint).Port, messages);
        }

        RunnerChannel(Socket connectSocket, int port, MessageQueue messages)
        {
            this.messages = messages;
            ConnectSocket = connectSocket;
            Port = port;
        }

        Socket Socket { get; set; }

        public void Send(Message message)
        {
            lock (writer)
            {
                writer.Write(JsonConvert.SerializeObject(message));
            }
        }

        void ReadMessagesIntoTheQueue()
        {
            while (true)
            {
                var rawMessage = reader.ReadString();
                var message = JsonConvert.DeserializeObject<Message>(rawMessage);

                messages.Add(message);

                if (message.MessageType == "TestRunner.TestCompleted")
                    break;
            }
        }

        public void EnqueueMessagesOnBackgroundThread()
        {
            new Thread(() =>
            {
                using (ConnectSocket)
                {
                    Socket = ConnectSocket.Accept();

                    var stream = new NetworkStream(Socket);
                    writer = new BinaryWriter(stream);
                    reader = new BinaryReader(stream);

                    new Thread(ReadMessagesIntoTheQueue) { IsBackground = true }.Start();
                }
            })
            {
                IsBackground = true
            }.Start();
        }

        public void Dispose()
        {
            Socket?.Dispose();
        }
    }
}