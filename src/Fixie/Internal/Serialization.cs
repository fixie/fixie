namespace Fixie.Internal
{
    using System.Text;

    #if NETCOREAPP3_0
    using System;
    using System.Text.Json;
    #else
    using System.IO;
    using System.Runtime.Serialization.Json;
    #endif

    static class Serialization
    {
        public static string Serialize<TMessage>(TMessage message)
        {
            return Encoding.UTF8.GetString(SerializeToBytes(message));
        }

        public static byte[] SerializeToBytes<TMessage>(TMessage message)
        {
            #if NETCOREAPP3_0

            return JsonSerializer.SerializeToUtf8Bytes(message);

            #else

            var serializer = new DataContractJsonSerializer(typeof(TMessage));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, message);
                return stream.ToArray();
            }

            #endif
        }

        public static TMessage Deserialize<TMessage>(string message)
        {
            return Deserialize<TMessage>(Encoding.UTF8.GetBytes(message));
        }

        public static TMessage Deserialize<TMessage>(byte[] bytes)
        {
            #if NETCOREAPP3_0

            return JsonSerializer.Deserialize<TMessage>(new ReadOnlySpan<byte>(bytes));

            #else

            var deserializer = new DataContractJsonSerializer(typeof(TMessage));

            using (var stream = new MemoryStream(bytes))
                return (TMessage) deserializer.ReadObject(stream);

            #endif
        }
    }
}