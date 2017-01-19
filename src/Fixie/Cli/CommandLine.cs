namespace Fixie.Cli
{
    using System;
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

                var positionalArguments = ScanPositionalArguments();

                var positionalArgumentValues = new List<string>();

                var queue = new Queue<string>(arguments);
                while (queue.Any())
                {
                    var item = queue.Dequeue();

                        if (positionalArgumentValues.Count >= positionalArguments.Count)
                            UnusedArguments.Add(item);
                        else
                            positionalArgumentValues.Add(item);
                }

                //If there are not enough argumentValues, assume default(T) for the missing ones.
                for (int i = 0; i < positionalArguments.Count; i++)
                {
                    positionalArguments[i].Value =
                        i < positionalArgumentValues.Count
                            ? positionalArgumentValues[i]
                            : Default(positionalArguments[i].Type);
                }

                foreach (var argument in positionalArguments)
                    argument.Value = Convert(argument.Type, argument.Name, argument.Value);

                Model = Create(positionalArguments);
            }

            static object Convert(Type type, string userFacingName, object value)
            {
                if (value == null)
                    return null;

                var conversionType = Nullable.GetUnderlyingType(type) ?? type;

                try
                {
                    return System.Convert.ChangeType(value, conversionType);
                }
                catch (Exception exception)
                {
                    throw new CommandLineException($"{userFacingName} was not in the correct format.", exception);
                }
            }

            static void DemandSingleConstructor()
            {
                if (typeof(T).GetConstructors().Length > 1)
                    throw new CommandLineException(
                        $"Parsing command line arguments for type {typeof(T).Name} " +
                        "is ambiguous, because it has more than one constructor.");
            }

            static List<PositionalArgument> ScanPositionalArguments()
                => GetConstructor()
                    .GetParameters()
                    .Select(p => new PositionalArgument(p))
                    .ToList();

            static T Create(List<PositionalArgument> arguments)
            {
                var model = Construct(arguments);

                return model;
            }

            static T Construct(List<PositionalArgument> arguments)
            {
                var parameters = arguments.Select(x => x.Value).ToArray();

                return (T)GetConstructor().Invoke(parameters);
            }


            static object Default(Type type)
                => type.IsValueType ? Activator.CreateInstance(type) : null;
            static ConstructorInfo GetConstructor()
                => typeof(T).GetConstructors().Single();
        }

        class PositionalArgument
        {
            public PositionalArgument(ParameterInfo parameter)
            {
                Type = parameter.ParameterType;
                Name = parameter.Name;
            }

            public Type Type { get; }
            public string Name { get; }
            public object Value { get; set; }
        }
    }
}