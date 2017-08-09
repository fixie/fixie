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

            var positionalArgumentValues = new List<string>();

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
                    if (positionalArgumentValues.Count >= positionalArguments.Count)
                        UnusedArguments.Add(item);
                    else
                        positionalArgumentValues.Add(item);
                }
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

            if (conversionType.IsEnum() && value is string)
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
            if (type.GetConstructors().Length > 1)
                throw new CommandLineException(
                    $"Parsing command line arguments for type {type.Name} " +
                    "is ambiguous, because it has more than one constructor.");
        }

        static List<PositionalArgument> ScanPositionalArguments(Type type)
            => GetConstructor(type)
                .GetParameters()
                .Select(p => new PositionalArgument(p))
                .ToList();

        static Dictionary<string, NamedArgument> ScanNamedArguments(Type type)
        {
            var namedArguments = type.GetProperties()
                .Where(p => p.CanWrite)
                .Select(p => new NamedArgument(p))
                .ToArray();

            var dictionary = new Dictionary<string, NamedArgument>();

            foreach (var namedArgument in namedArguments)
            {
                if (dictionary.ContainsKey(namedArgument.Name))
                    throw new CommandLineException(
                        $"Parsing command line arguments for type {type.Name} " +
                        "is ambiguous, because it has more than one property corresponding " +
                        $"with the --{namedArgument.Name} argument.");

                dictionary.Add(namedArgument.Name, namedArgument);
            }

            return dictionary;
        }

        static object Create(Type type, List<PositionalArgument> arguments, Dictionary<string, NamedArgument> namedArguments)
        {
            var model = Construct(type, arguments);

            Populate(type, model, namedArguments);

            return model;
        }

        static object Construct(Type type, List<PositionalArgument> arguments)
        {
            var parameters = arguments.Select(x => x.Value).ToArray();

            return GetConstructor(type).Invoke(parameters);
        }

        static void Populate(Type type, object instance, Dictionary<string, NamedArgument> namedArguments)
        {
            foreach (var name in namedArguments.Keys)
            {
                var namedArgument = namedArguments[name];

                var property = type.GetProperty(namedArgument.PropertyName);

                if (namedArgument.IsArray)
                    property.SetValue(instance, namedArgument.CreateTypedValuesArray());
                else if (namedArgument.Values.Count != 0)
                    property.SetValue(instance, namedArgument.Values.Single());
            }
        }

        static object Default(Type type)
            => type.IsValueType() ? Activator.CreateInstance(type) : null;

        static ConstructorInfo GetConstructor(Type type)
            => type.GetConstructors().Single();

        static bool IsNamedArgumentKey(string item)
            => item.StartsWith("--");
    }
}