using System.Reflection;

namespace Fixie.Console;

class Parser<T>
{
    public T Model { get; }

    public Parser(string[] arguments)
    {
        var type = typeof(T);

        DemandSingleConstructor(type);

        var positionalArguments = ScanPositionalArguments(type);
        var namedArguments = ScanNamedArguments(type);

        List<object?> paramsPositionalArgumentValues = [];

        var queue = new Queue<string>(arguments);
        while (queue.Any())
        {
            var item = queue.Dequeue();

            if (IsFirstLetterAbbreviation(item))
                item = ExpandFirstLetterAbbreviation(namedArguments, item);

            if (IsNamedArgumentKey(item))
            {
                var name = NamedArgument.Normalize(item);

                if (!namedArguments.ContainsKey(name))
                    throw new CommandLineException("Unexpected argument: " + item);

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
                    throw new CommandLineException("Unexpected argument: " + item);
            }
        }

        foreach (var positionalArgument in positionalArguments)
        {
            //Bind all of paramsPositionalArgumentValues to this argument.
            var itemType = positionalArgument.Type.GetElementType()!;

            paramsPositionalArgumentValues =
                paramsPositionalArgumentValues
                    .Select(x =>
                        Convert(itemType, positionalArgument.Name, x)).ToList();

            positionalArgument.Value = CreateTypedArray(itemType, paramsPositionalArgumentValues);
        }

        Model = Create(type, positionalArguments, namedArguments);
    }

    static object? Convert(Type type, string userFacingName, object? value)
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
                    string.Join(", ",
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
                .Select(p => new NamedArgument(p.ParameterType, p.Name!))
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

    static string ExpandFirstLetterAbbreviation(Dictionary<string, NamedArgument> namedArguments, string item)
    {
        var candidates =
            namedArguments.Keys
                .Where(x => x.StartsWith(NamedArgument.Normalize(item)))
                .OrderBy(x => x)
                .ToArray();

        if (candidates.Length > 1)
        {
            var suggestions = candidates.Select(x => $"--{x}").ToArray();

            suggestions[^1] = $"or {suggestions[^1]}";

            throw new CommandLineException(
                $"{item} is not a recognized option. Did you mean " +
                $"{string.Join(suggestions.Length > 2 ? ", " : " ", suggestions)}?");
        }

        if (candidates.Length == 0)
            throw new CommandLineException("Unexpected argument: " + item);

        return $"--{candidates.Single()}";
    }

    static T Create(Type type, List<PositionalArgument> arguments, Dictionary<string, NamedArgument> namedArguments)
    {
        var constructor = GetConstructor(type);

        var declaredParameters = constructor.GetParameters();

        List<object?> actualParameters = [];

        foreach (var declaredParameter in declaredParameters)
        {
            var named = declaredParameter.GetCustomAttribute<ParamArrayAttribute>() == null;

            if (named)
            {
                var key = NamedArgument.Normalize(declaredParameter.Name!);

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

        return (T)constructor.Invoke(actualParameters.ToArray());
    }

    static object? Default(Type type)
        => type.IsValueType ? Activator.CreateInstance(type) : null;

    static ConstructorInfo GetConstructor(Type type)
        => type.GetConstructors().Single();

    static bool IsFirstLetterAbbreviation(string item)
        => item.Length == 2 && item[0] == '-' && char.IsLetter(item[1]);

    static bool IsNamedArgumentKey(string item)
        => item.StartsWith("-");

    static Array CreateTypedArray(Type itemType, IReadOnlyList<object?> values)
    {
        Array destinationArray = Array.CreateInstance(itemType, values.Count);
        Array.Copy(values.ToArray(), destinationArray, values.Count);
        return destinationArray;
    }
}