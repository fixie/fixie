namespace Fixie.Execution.Listeners
{
    using System.IO;
    using System.IO.Pipes;
    using System.Runtime.Serialization.Json;
    using System.Text;

    static class PipeStreamExtensions
    {
        public static TMessage Receive<TMessage>(this PipeStream pipe)
        {
            var deserializer = new DataContractJsonSerializer(typeof(TMessage));

            using (var stream = new MemoryStream(ReceiveMessageBytes(pipe)))
                return (TMessage)deserializer.ReadObject(stream);
        }

        public static void Send<TMessage>(this PipeStream pipe, TMessage message)
        {
            var serializer = new DataContractJsonSerializer(typeof(TMessage));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, message);

                SendMessageBytes(pipe, stream.ToArray());
            }
        }

        public static string ReceiveMessage(this PipeStream pipe)
        {
            return Encoding.UTF8.GetString(ReceiveMessageBytes(pipe));
        }

        public static void Send(this PipeStream pipe, PipeCommand command)
        {
            SendMessage(pipe, command.ToString());
        }

        public static void SendMessage(this PipeStream pipe, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            SendMessageBytes(pipe, bytes);
        }

        static byte[] ReceiveMessageBytes(PipeStream pipe)
        {
            var buffer = new byte[1024];

            using (var ms = new MemoryStream())
            {
                do
                {
                    var byteCount = pipe.Read(buffer, 0, buffer.Length);

                    if (byteCount > 0)
                        ms.Write(buffer, 0, byteCount);
                }
                while (!pipe.IsMessageComplete);

                return ms.ToArray();
            }
        }

        static void SendMessageBytes(PipeStream pipe, byte[] bytes)
        {
            pipe.Write(bytes, 0, bytes.Length);
        }
    }
}