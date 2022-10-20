namespace Fixie.Internal
{
    using System.Text;
    using System.Text.Json;

    static class Serialization
    {
        public static string Serialize<TMessage>(TMessage message)
            => Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(message));

        public static TMessage Deserialize<TMessage>(string message)
            => JsonSerializer.Deserialize<TMessage>(Encoding.UTF8.GetBytes(message))!;
    }
}