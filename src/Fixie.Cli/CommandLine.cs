namespace Fixie.Cli
{
    using System;
    using System.Collections.Generic;

    class CommandLine
    {
        public static T Parse<T>(string[] arguments) where T : class
            => (T)Parse(typeof(T), arguments);

        public static object Parse(Type type, string[] arguments)
        {
            var parser = new Parser(type, arguments);

            return parser.Model;
        }

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