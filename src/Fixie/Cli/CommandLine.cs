namespace Fixie.Cli
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CommandLine
    {
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
                UnusedArguments = new List<string>();
                UnusedArguments.AddRange(arguments);
                Model = Create();
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