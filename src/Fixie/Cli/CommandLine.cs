namespace Fixie.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CommandLine
    {
        public static T Parse<T>(string[] arguments) where T : class
        {
            string[] unusedArguments;
            return Parse<T>(arguments, out unusedArguments);
        }

        public static T Parse<T>(string[] arguments, out string[] unusedArguments) where T : class
        {
            var parser = new Parser<T>(arguments);

            unusedArguments = parser.UnusedArguments.ToArray();

            return parser.Model;
        }

        class Parser<T> where T : class
        {
            public T Model { get; }
            public List<string> UnusedArguments { get; }

            public Parser(string[] args)
            {
                DemandSingleConstructor();
                UnusedArguments = new List<string>();

                var arguments = ScanArguments();
                var options = ScanOptions();

                var argumentValues = new List<string>();

                var queue = new Queue<string>(args);
                while (queue.Any())
                {
                    var item = queue.Dequeue();

                    if (IsOption(item))
                    {
                        var name = Option.Normalize(item);

                        if (!options.ContainsKey(name))
                        {
                            UnusedArguments.Add(item);

                            if (queue.Any() && !IsOption(queue.Peek()))
                                UnusedArguments.Add(queue.Dequeue());

                            continue;
                        }

                        var option = options[name];

                        object value;

                        bool requireValue = option.ItemType != typeof(bool);

                        if (requireValue)
                        {
                            if (!queue.Any() || IsOption(queue.Peek()))
                                throw new CommandLineException($"Option {item} is missing its required value.");

                            value = queue.Dequeue();
                        }
                        else
                        {
                            value = true;
                        }

                        if (option.Values.Count == 1 && !option.IsArray)
                            throw new CommandLineException($"Option {item} cannot be specified more than once.");

                        option.Values.Add(ConvertOrDefault(option.ItemType, item, value));
                    }
                    else
                    {
                        if (argumentValues.Count >= arguments.Count)
                            UnusedArguments.Add(item);
                        else
                            argumentValues.Add(item);
                    }
                }

                //If there are not enough argumentValues, assume default(T) for the missing ones.
                for (int i = 0; i < arguments.Count; i++)
                {
                    arguments[i].Value =
                        i < argumentValues.Count
                            ? argumentValues[i]
                            : Default(arguments[i].Type);
                }

                foreach (var argument in arguments)
                    argument.Value = ConvertOrDefault(argument.Type, argument.Name, argument.Value);

                Model = Create(arguments, options);
            }

            static object ConvertOrDefault(Type type, string userFacingName, object value)
            {
                if (type == typeof(bool?) || type == typeof(bool))
                {
                    var stringValue = value as string;
                    if (value != null)
                    {
                        if (stringValue == "on" || stringValue == "true")
                            value = true;
                        else if (stringValue == "off" || stringValue == "false")
                            value = false;
                    }
                }

                if (value == null)
                    return null;

                var conversionType = Nullable.GetUnderlyingType(type) ?? type;

                if (conversionType.IsEnum && value is string)
                {
                    try
                    {
                        return Enum.Parse(conversionType, (string)value, ignoreCase: true);
                    }
                    catch (Exception exception)
                    {
                        var allowedValues =
                            String.Join(", ",
                                Enum.GetValues(conversionType)
                                    .Cast<object>()
                                    .Select(x => x.ToString()));

                        throw new CommandLineException($"Expected {userFacingName} to be one of: {allowedValues}.", exception);
                    }
                }

                try
                {
                    return Convert.ChangeType(value, conversionType);
                }
                catch (Exception exception)
                {
                    var expectedTypeName = ExpectedTypeName(type);
                    throw new CommandLineException($"Expected {userFacingName} to be convertible to {expectedTypeName}.", exception);
                }
            }

            static string ExpectedTypeName(Type expected)
            {
                var nullableUnderlyingType = Nullable.GetUnderlyingType(expected);
                var suffix = nullableUnderlyingType == null ? "" : "?";
                expected = nullableUnderlyingType ?? expected;

                if (expected == typeof(bool)) return "bool" + suffix;
                if (expected == typeof(byte)) return "byte" + suffix;
                if (expected == typeof(char)) return "char" + suffix;
                if (expected == typeof(decimal)) return "decimal" + suffix;
                if (expected == typeof(double)) return "double" + suffix;
                if (expected == typeof(float)) return "float" + suffix;
                if (expected == typeof(int)) return "int" + suffix;
                if (expected == typeof(long)) return "long" + suffix;
                if (expected == typeof(sbyte)) return "sbyte" + suffix;
                if (expected == typeof(short)) return "short" + suffix;
                if (expected == typeof(uint)) return "uint" + suffix;
                if (expected == typeof(ulong)) return "ulong" + suffix;
                if (expected == typeof(ushort)) return "ushort" + suffix;
                if (expected == typeof(string)) return "string";
                return expected.FullName;
            }

            static void DemandSingleConstructor()
            {
                if (typeof(T).GetConstructors().Length > 1)
                    throw new CommandLineException(
                        $"Parsing command line arguments for type {typeof(T).Name} " +
                        "is ambiguous, because it has more than one constructor.");
            }

            static List<Argument> ScanArguments()
                => GetConstructor()
                    .GetParameters()
                    .Select(p => new Argument(p))
                    .ToList();

            static Dictionary<string, Option> ScanOptions()
            {
                var options = typeof(T).GetProperties()
                    .Where(p => p.CanWrite)
                    .Select(p => new Option(p))
                    .ToArray();

                var dictionary = new Dictionary<string, Option>();
                foreach (var option in options)
                {
                    if (dictionary.ContainsKey(option.Name))
                        throw new CommandLineException(
                            $"Parsing command line arguments for type {typeof(T).Name} " +
                            "is ambiguous, because it has more than one property corresponding " +
                            $"with the --{option.Name} option.");

                    dictionary.Add(option.Name, option);
                }
                return dictionary;
            }

            static T Create(List<Argument> arguments, Dictionary<string, Option> options)
            {
                var model = Construct(arguments);

                Populate(model, options);

                return model;
            }

            static void Populate(T instance, Dictionary<string, Option> options)
            {
                foreach (var name in options.Keys)
                {
                    var option = options[name];

                    var property = typeof(T).GetProperty(option.PropertyName);

                    if (option.IsArray)
                        property.SetValue(instance, option.CreateTypedValuesArray());
                    else if (option.Values.Count != 0)
                        property.SetValue(instance, option.Values.Single());
                }
            }

            static T Construct(List<Argument> arguments)
            {
                var parameters = arguments.Select(x => x.Value).ToArray();

                return (T)GetConstructor().Invoke(parameters);
            }

            static object Default(Type type)
                => type.IsValueType ? Activator.CreateInstance(type) : null;

            static ConstructorInfo GetConstructor()
                => typeof(T).GetConstructors().Single();

            static bool IsOption(string item)
                => item.StartsWith("--");
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

        class Option
        {
            public Option(PropertyInfo property)
            {
                IsArray = property.PropertyType.IsArray;

                ItemType = IsArray
                    ? property.PropertyType.GetElementType()
                    : property.PropertyType;

                PropertyName = property.Name;
                Name = Normalize(PropertyName);
                Values = new List<object>();
            }

            public bool IsArray { get; }
            public Type ItemType { get; }
            public string PropertyName { get; }
            public string Name { get; }
            public List<object> Values { get; }

            public Array CreateTypedValuesArray()
            {
                Array destinationArray = Array.CreateInstance(ItemType, Values.Count);
                Array.Copy(Values.ToArray(), destinationArray, Values.Count);
                return destinationArray;
            }

            public static string Normalize(string option)
                => option.ToLower().Replace("-", "");
        }
    }
}