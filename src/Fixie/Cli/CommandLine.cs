namespace Fixie.Cli
{
    class CommandLine
    {
        public static T Parse<T>(string[] arguments) where T : class
        {
            return Parse<T>(arguments, out string[] _);
        }

        public static T Parse<T>(string[] arguments, out string[] unusedArguments) where T : class
        {
            var parser = new Parser<T>(arguments);

            unusedArguments = parser.UnusedArguments.ToArray();

            return parser.Model;
        }

        public static string Serialize(string[] arguments)
        {
            return Serializer.Serialize(arguments);
        }
    }
}