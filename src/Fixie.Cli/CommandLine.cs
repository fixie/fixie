namespace Fixie.Cli
{
    using System;
    using System.Collections.Generic;

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

        public static void Partition(string[] arguments, out string[] runnerArguments, out string[] customArguments)
        {
            var runnerArgumentsList = new List<string>();
            var customArgumentsList = new List<string>();

            bool separatorFound = false;
            foreach (var arg in arguments)
            {
                if (arg == "--")
                {
                    separatorFound = true;
                    continue;
                }

                if (separatorFound)
                    customArgumentsList.Add(arg);
                else
                    runnerArgumentsList.Add(arg);
            }

            runnerArguments = runnerArgumentsList.ToArray();
            customArguments = customArgumentsList.ToArray();
        }
    }
}