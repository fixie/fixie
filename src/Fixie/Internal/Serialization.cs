namespace Fixie.Internal
{
    using System;
    using System.Text;
    using System.Text.Json;

    static class Serialization
    {
        public static string Serialize<TMessage>(TMessage message)
            => Encoding.UTF8.GetString(SerializeToBytes(message));

        public static byte[] SerializeToBytes<TMessage>(TMessage message)
            => JsonSerializer.SerializeToUtf8Bytes(message);

        public static TMessage? Deserialize<TMessage>(string message) where TMessage : class
            => Deserialize<TMessage>(Encoding.UTF8.GetBytes(message));

        public static TMessage? Deserialize<TMessage>(byte[] bytes) where TMessage : class
            => JsonSerializer.Deserialize<TMessage>(new ReadOnlySpan<byte>(bytes));
    }
}