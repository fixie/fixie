namespace Fixie.Cli
{
    using System;

    class CommandLine
    {
        public static T Parse<T>(string[] arguments) where T : class
            => (T)Parse(typeof(T), arguments);

        public static T Parse<T>(string[] arguments, out string[] unusedArguments) where T : class
            => (T)Parse(typeof(T), arguments, out unusedArguments);

        public static object Parse(Type type, string[] arguments)
            => Parse(type, arguments, out string[] _);

        public static object Parse(Type type, string[] arguments, out string[] unusedArguments)
        {
            var parser = new Parser(type, arguments);

            unusedArguments = parser.UnusedArguments.ToArray();

            return parser.Model;
        }

        public static string Serialize(string[] arguments)
            => Serializer.Serialize(arguments);
    }
}