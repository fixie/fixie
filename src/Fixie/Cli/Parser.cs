namespace Fixie.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class Parser
    {
        public object Model { get; }
        public List<string> UnusedArguments { get; }

        public Parser(Type type, string[] arguments)
        {
            DemandSingleConstructor(type);
            UnusedArguments = new List<string>();

            var positionalArguments = ScanPositionalArguments(type);
            var namedArguments = ScanNamedArguments(type);

            var paramsPositionalArgumentValues = new List<object>();

            var queue = new Queue<string>(arguments);
            while (queue.Any())
            {
                var item = queue.Dequeue();

                if (IsNamedArgumentKey(item))
                {
                    var name = NamedArgument.Normalize(item);

                    if (!namedArguments.ContainsKey(name))
                    {
                        UnusedArguments.Add(item);

                        if (queue.Any() && !IsNamedArgumentKey(queue.Peek()))
                            UnusedArguments.Add(queue.Dequeue());

                        continue;
                    }

                    var namedArgument = namedArguments[name];

                    object value;

                    bool requireValue = namedArgument.ItemType != typeof(bool);

                    if (requireValue)
                    {
                        if (!queue.Any() || IsNamedArgumentKey(queue.Peek()))
                            throw new CommandLineException($"{item} is missing its required value.");

                        value = queue.Dequeue();
                    }
                    else
                    {
                        value = true;
                    }

                    if (namedArgument.Values.Count == 1 && !namedArgument.IsArray)
                        throw new CommandLineException($"{item} cannot be specified more than once.");

                    namedArgument.Values.Add(Convert(namedArgument.ItemType, item, value));
                }
                else
                {
                    if (positionalArguments.Any())
                        paramsPositionalArgumentValues.Add(item);
                    else
                        UnusedArguments.Add(item);
                }
            }

            foreach (var positionalArgument in positionalArguments)
            {
                //Bind all of paramsPositionalArgumentValues to this argument.
                var itemType = positionalArgument.Type.GetElementType();

                paramsPositionalArgumentValues =
                    paramsPositionalArgumentValues
                        .Select(x =>
                            Convert(itemType, positionalArgument.Name, x)).ToList();

                positionalArgument.Value = CreateTypedArray(itemType, paramsPositionalArgumentValues);
            }

            Model = Create(type, positionalArguments, namedArguments);
        }

        static object Convert(Type type, string userFacingName, object value)
        {
            if (type == typeof(bool?) || type == typeof(bool))
            {
                var stringValue = value as string;
                if (value != null)
                {
                    stringValue = stringValue?.ToLower();

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

                    throw new CommandLineException($"{userFacingName} must be one of: {allowedValues}.", exception);
                }
            }

            try
            {
                return System.Convert.ChangeType(value, conversionType);
            }
            catch (Exception exception)
            {
                throw new CommandLineException($"{userFacingName} was not in the correct format.", exception);
            }
        }

        static void DemandSingleConstructor(Type type)
        {
            var constructors = type.GetConstructors().Length;

            if (constructors > 1)
                throw new CommandLineException(
                    $"Could not construct an instance of type '{type.FullName}'. Expected to find exactly 1 public constructor, but found {constructors}.");
        }

        static List<PositionalArgument> ScanPositionalArguments(Type type)
            => GetConstructor(type)
                .GetParameters()
                .Where(p => p.GetCustomAttribute<ParamArrayAttribute>() != null)
                .Select(p => new PositionalArgument(p))
                .ToList();

        static Dictionary<string, NamedArgument> ScanNamedArguments(Type type)
        {
            var namedArguments =
                GetConstructor(type)
                .GetParameters()
                .Where(p => p.GetCustomAttribute<ParamArrayAttribute>() == null)
                .Select(p => new NamedArgument(p.ParameterType, p.Name))
                .ToArray();

            var dictionary = new Dictionary<string, NamedArgument>();

            foreach (var namedArgument in namedArguments)
            {
                if (dictionary.ContainsKey(namedArgument.Name))
                    throw new CommandLineException(
                        $"Parsing command line arguments for type {type.Name} " +
                        "is ambiguous, because it has more than one parameter corresponding " +
                        $"with the --{namedArgument.Name} argument.");

                dictionary.Add(namedArgument.Name, namedArgument);
            }

            return dictionary;
        }

        static object Create(Type type, List<PositionalArgument> arguments, Dictionary<string, NamedArgument> namedArguments)
        {
            var constructor = GetConstructor(type);

            var declaredParameters = constructor.GetParameters();

            var actualParameters = new List<object>();

            foreach (var declaredParameter in declaredParameters)
            {
                var named = declaredParameter.GetCustomAttribute<ParamArrayAttribute>() == null;

                if (named)
                {
                    var key = NamedArgument.Normalize(declaredParameter.Name);

                    var namedArgument = namedArguments[key];

                    if (namedArgument.IsArray)
                        actualParameters.Add(CreateTypedArray(namedArgument.ItemType, namedArgument.Values));
                    else if (namedArgument.Values.Count != 0)
                        actualParameters.Add(namedArgument.Values.Single());
                    else
                        actualParameters.Add(Default(declaredParameter.ParameterType));
                }
                else
                {
                    actualParameters.Add(arguments.Single().Value);
                }
            }

            return constructor.Invoke(actualParameters.ToArray());
        }

        static object Default(Type type)
            => type.IsValueType ? Activator.CreateInstance(type) : null;

        static ConstructorInfo GetConstructor(Type type)
            => type.GetConstructors().Single();

        static bool IsNamedArgumentKey(string item)
            => item.StartsWith("--");

        static Array CreateTypedArray(Type itemType, IReadOnlyList<object> values)
        {
            Array destinationArray = Array.CreateInstance(itemType, values.Count);
            Array.Copy(values.ToArray(), destinationArray, values.Count);
            return destinationArray;
        }
    }
}