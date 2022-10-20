namespace Fixie.Internal
{
    using System.Text.Json;

    static class Serialization
    {
        public static string Serialize<TMessage>(TMessage message)
            => JsonSerializer.Serialize(message);

        public static TMessage Deserialize<TMessage>(string message)
            => JsonSerializer.Deserialize<TMessage>(message)!;
    }
}