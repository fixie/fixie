namespace Fixie.Internal.Listeners
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Text;
    using static Serialization;

    static class PipeStreamExtensions
    {
        public static TMessage Receive<TMessage>(this PipeStream pipe)
        {
            return Deserialize<TMessage>(ReceiveMessageBytes(pipe));
        }

        public static void Send<TMessage>(this PipeStream pipe) where TMessage: new()
        {
           pipe.Send(new TMessage());
        }

        public static void Send(this PipeStream pipe, Exception exception)
        {
            pipe.Send(new PipeMessage.Exception(exception));
        }

        public static void Send<TMessage>(this PipeStream pipe, TMessage message)
        {
            var messageType = typeof(TMessage).FullName;
            SendMessageBytes(pipe, Encoding.UTF8.GetBytes(messageType));

            SendMessageBytes(pipe, SerializeToBytes(message));
        }

        public static string ReceiveMessage(this PipeStream pipe)
        {
            return Encoding.UTF8.GetString(ReceiveMessageBytes(pipe));
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