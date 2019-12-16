namespace Fixie.Internal
{
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    static class Serialization
    {
        public static string Serialize<TMessage>(TMessage message)
        {
            var serializer = new DataContractJsonSerializer(typeof(TMessage));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, message);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static TMessage Deserialize<TMessage>(string message)
        {
            return Deserialize<TMessage>(Encoding.UTF8.GetBytes(message));
        }

        public static TMessage Deserialize<TMessage>(byte[] bytes)
        {
            var deserializer = new DataContractJsonSerializer(typeof(TMessage));

            using (var stream = new MemoryStream(bytes))
                return (TMessage) deserializer.ReadObject(stream);
        }
    }
}