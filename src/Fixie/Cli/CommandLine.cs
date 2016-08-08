namespace Fixie.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CommandLine
    {
        public static ParseResult<T> Parse<T>(params string[] args) where T : class
        {
            var parser = new Parser<T>(args);

            return new ParseResult<T>(parser.Model, parser.ExtraArguments, parser.Errors);
        }

        class Parser<T> where T : class
        {
            public T Model { get; }
            public List<string> ExtraArguments { get; }
            public List<string> Errors { get; }

            public Parser(string[] args)
            {
                DemandSingleConstructor<T>();
                Errors = new List<string>();

                var arguments = ScanArguments<T>();

                var argumentValues = new List<string>();

                var queue = new Queue<string>(args);
                while (queue.Any())
                {
                    var item = queue.Dequeue();

                    argumentValues.Add(item);
                }

                //If there are not enough argumentValues, assume default(T) for the missing ones.
                for (int i = 0; i < arguments.Count; i++)
                {
                    arguments[i].Value =
                        i < argumentValues.Count
                            ? argumentValues[i]
                            : Default(arguments[i].Type);
                }

                //If the argument types are not convertible, note the error and assume default(T).
                foreach (var argument in arguments)
                {
                    try
                    {
                        argument.Value = Convert.ChangeType(argument.Value, argument.Type);
                    }
                    catch (Exception)
                    {
                        Errors.Add($"Expected {argument.Name} to be convertible to {argument.Type.FullName}.");
                        argument.Value = Default(argument.Type);
                    }
                }

                Model = Create<T>(arguments);
                ExtraArguments = argumentValues.Skip(arguments.Count).ToList();
            }

            static void DemandSingleConstructor<T>()
            {
                if (typeof(T).GetConstructors().Length > 1)
                    throw new Exception(
                        $"Parsing command line arguments for type {typeof(T).Name} " +
                        "is ambiguous, because it has more than one constructor.");
            }

            static List<Argument> ScanArguments<T>() where T : class
                => GetConstructor<T>()
                    .GetParameters()
                    .Select(p => new Argument(p))
                    .ToList();

            static T Create<T>(List<Argument> arguments) where T : class
            {
                var model = Construct<T>(arguments);

                return model;
            }

            static T Construct<T>(List<Argument> arguments) where T : class
            {
                var parameters = arguments.Select(x => x.Value).ToArray();

                return (T)GetConstructor<T>().Invoke(parameters);
            }

            static object Default(Type type)
                => type.IsValueType ? Activator.CreateInstance(type) : null;

            static ConstructorInfo GetConstructor<T>() where T : class
                => typeof(T).GetConstructors().Single();
        }

        class Argument
        {
            public Argument(ParameterInfo parameter)
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