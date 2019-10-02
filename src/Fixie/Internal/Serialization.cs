namespace Fixie.Internal
{
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    static class Serialization
    {
        public static string Serialize<T>(T message)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, message);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static T Deserialize<T>(string message)
        {
            var deserializer = new DataContractJsonSerializer(typeof(T));

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
                return (T)deserializer.ReadObject(stream);
        }
    }
}