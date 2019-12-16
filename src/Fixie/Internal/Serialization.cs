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
            var bytes = Encoding.UTF8.GetBytes(message);

            var deserializer = new DataContractJsonSerializer(typeof(TMessage));

            using (var stream = new MemoryStream(bytes))
                return (TMessage)deserializer.ReadObject(stream);
        }
    }
}