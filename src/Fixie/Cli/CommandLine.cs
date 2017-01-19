namespace Fixie.Cli
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CommandLine
    {
        public static T Parse<T>(IReadOnlyList<string> arguments) where T : class
        {
            string[] unusedArguments;
            return Parse<T>(arguments, out unusedArguments);
        }

        public static T Parse<T>(IReadOnlyList<string> arguments, out string[] unusedArguments) where T : class
        {
            var parser = new Parser<T>(arguments.ToArray());

            unusedArguments = parser.UnusedArguments.ToArray();

            return parser.Model;
        }

        class Parser<T> where T : class
        {
            public T Model { get; }
            public List<string> UnusedArguments { get; }

            public Parser(string[] arguments)
            {
                DemandSingleConstructor();
                UnusedArguments = new List<string>();
                UnusedArguments.AddRange(arguments);
                Model = Create();
            }


            static void DemandSingleConstructor()
            {
                if (typeof(T).GetConstructors().Length > 1)
                    throw new CommandLineException(
                        $"Parsing command line arguments for type {typeof(T).Name} " +
                        "is ambiguous, because it has more than one constructor.");
            }

            static T Create()
            {
                var model = Construct();

                return model;
            }

            static T Construct()
            {
                var parameters = new object[] { };

                return (T)GetConstructor().Invoke(parameters);
            }

            static ConstructorInfo GetConstructor()
                => typeof(T).GetConstructors().Single();
        }
    }
}