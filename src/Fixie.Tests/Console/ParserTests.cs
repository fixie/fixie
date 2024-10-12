using System.Runtime.CompilerServices;
using System.Text.Json;
using Fixie.Console;

namespace Fixie.Tests.Console;

public class ParserTests
{
    enum Level { Information, Warning, Error }

    class Empty { }

    public void ShouldParseEmptyModels()
    {
        Parse<Empty>().ShouldSucceed(new Empty());
    }

    class TooManyConstructors
    {
        public TooManyConstructors() { }
        public TooManyConstructors(int x) { }
    }

    public void ShouldDemandExactlyOneConstructor()
    {
        Parse<TooManyConstructors>()
            .ShouldFail($"Could not construct an instance of type '{typeof(TooManyConstructors).FullName}'. " +
                        "Expected to find exactly 1 public constructor, but found 2.");
    }

    class ModelWithConstructor<T>
    {
        public T First { get; }
        public T Second { get; }
        public T Third { get; }

        public ModelWithConstructor(T first, T second, T third)
        {
            First = first;
            Second = second;
            Third = third;
        }
    }

    class ModelWithParams<T>
    {
        public T First { get; }
        public T Second { get; }
        public T Third { get; }
        public T[] Rest { get; }

        public ModelWithParams(T first, T second, T third, params T[] rest)
        {
            First = first;
            Second = second;
            Third = third;
            Rest = rest;
        }
    }

    public void ShouldParseArgumentsAsConstructorParameters()
    {
        Parse<ModelWithConstructor<string>>("--first", "value1", "--second", "value2", "--third", "value3")
            .ShouldSucceed(new ModelWithConstructor<string>("value1", "value2", "value3"));

        Parse<ModelWithConstructor<int>>("--first", "1", "--second", "2", "--third", "3")
            .ShouldSucceed(new ModelWithConstructor<int>(1, 2, 3));

        Parse<ModelWithConstructor<int>>("--first", "1", "--second", "2", "--third", "abc")
            .ShouldFail("--third was not in the correct format.");

        Parse<ModelWithConstructor<string>>("-f", "value1", "-s", "value2", "-t", "value3")
            .ShouldSucceed(new ModelWithConstructor<string>("value1", "value2", "value3"));

        Parse<ModelWithConstructor<int>>("-f", "1", "-s", "2", "-t", "3")
            .ShouldSucceed(new ModelWithConstructor<int>(1, 2, 3));

        Parse<ModelWithConstructor<int>>("-f", "1", "-s", "2", "-t", "abc")
            .ShouldFail("--third was not in the correct format.");

        //Unspecified params[]
        Parse<ModelWithParams<string>>("--first", "value1", "--second", "value2", "--third", "value3")
            .ShouldSucceed(new ModelWithParams<string>("value1", "value2", "value3"));

        Parse<ModelWithParams<int>>("--first", "1", "--second", "2", "--third", "3")
            .ShouldSucceed(new ModelWithParams<int>(1, 2, 3));

        Parse<ModelWithParams<int>>("--first", "1", "--second", "2", "--third", "abc")
            .ShouldFail("--third was not in the correct format.");

        //Specified params[]
        Parse<ModelWithParams<string>>("--first", "first", "--second", "second", "--third", "third", "fourth", "fifth")
            .ShouldSucceed(new ModelWithParams<string>("first", "second", "third", "fourth", "fifth"));

        Parse<ModelWithParams<int>>("--first", "1", "--second", "2", "--third", "3", "4", "5")
            .ShouldSucceed(new ModelWithParams<int>(1, 2, 3, 4, 5));

        Parse<ModelWithParams<int>>("--first", "1", "--second", "2", "--third", "3", "4", "abc")
            .ShouldFail("rest was not in the correct format.");
    }

    public void ShouldFailWhenNamedArgumentsAreMissingTheirRequiredValues()
    {
        Parse<ModelWithConstructor<int>>("--first", "1", "--second", "2", "--third")
            .ShouldFail("--third is missing its required value.");
        
        Parse<ModelWithConstructor<int>>("-f", "1", "-s", "2", "-t")
            .ShouldFail("--third is missing its required value.");

        Parse<ModelWithConstructor<int>>("--first", "1", "--second", "--third", "3")
            .ShouldFail("--second is missing its required value.");

        Parse<ModelWithConstructor<int>>("-f", "1", "-s", "-t", "3")
            .ShouldFail("--second is missing its required value.");
    }

    public void ShouldLeaveDefaultValuesForMissingNamedArguments()
    {
        Parse<ModelWithConstructor<string?>>("--first", "value1", "--second", "value2")
            .ShouldSucceed(new ModelWithConstructor<string?>("value1", "value2", null));

        Parse<ModelWithConstructor<int>>("--first", "1", "--second", "2")
            .ShouldSucceed(new ModelWithConstructor<int>(1, 2, 0));

        //Unspecified params[] default to an empty array.
        Parse<ModelWithParams<string?>>("--first", "first", "--second", "second")
            .ShouldSucceed(new ModelWithParams<string?>("first", "second", null));

        Parse<ModelWithParams<int>>("--first", "1", "--second", "2")
            .ShouldSucceed(new ModelWithParams<int>(1, 2, 0));
    }

    public void ShouldParseNullableValueTypeArguments()
    {
        Parse<ModelWithConstructor<int?>>("--first", "1", "--third", "3")
            .ShouldSucceed(new ModelWithConstructor<int?>(1, null, 3));

        Parse<ModelWithConstructor<char?>>("--first", "a", "--third", "c")
            .ShouldSucceed(new ModelWithConstructor<char?>('a', null, 'c'));

        //Unspecified params[]
        Parse<ModelWithParams<int?>>("--first", "1", "--third", "3")
            .ShouldSucceed(new ModelWithParams<int?>(1, null, 3));

        Parse<ModelWithParams<char?>>("--first", "a", "--third", "c")
            .ShouldSucceed(new ModelWithParams<char?>('a', null, 'c'));

        //Specified params[]
        Parse<ModelWithParams<int?>>("--first", "1", "--third", "3", "4", "5")
            .ShouldSucceed(new ModelWithParams<int?>(1, null, 3, 4, 5));

        Parse<ModelWithParams<char?>>("--first", "a", "--third", "c", "d", "e")
            .ShouldSucceed(new ModelWithParams<char?>('a', null, 'c', 'd', 'e'));
    }

    public void ShouldParseBoolArgumentsAsFlagsWithoutExplicitValues()
    {
        Parse<ModelWithConstructor<bool>>("--first", "--third")
            .ShouldSucceed(new ModelWithConstructor<bool>(true, false, true));
        
        Parse<ModelWithConstructor<bool>>("-f", "-t")
            .ShouldSucceed(new ModelWithConstructor<bool>(true, false, true));

        //Unspecified params[]
        Parse<ModelWithParams<bool>>("--first", "--third")
            .ShouldSucceed(new ModelWithParams<bool>(true, false, true));

        //Specified params[]
        Parse<ModelWithParams<bool>>("--first", "--third", "true", "false")
            .ShouldSucceed(new ModelWithParams<bool>(true, false, true, true, false));

        Parse<ModelWithParams<bool>>("--first", "--third", "FALSE", "TRUE")
            .ShouldSucceed(new ModelWithParams<bool>(true, false, true, false, true));

        Parse<ModelWithParams<bool>>("--first", "--third", "on", "off")
            .ShouldSucceed(new ModelWithParams<bool>(true, false, true, true, false));

        Parse<ModelWithParams<bool>>("--first", "--third", "OFF", "ON")
            .ShouldSucceed(new ModelWithParams<bool>(true, false, true, false, true));

        Parse<ModelWithParams<bool>>("--first", "--third", "value")
            .ShouldFail("rest was not in the correct format.");
    }

    public void ShouldParseNullableBoolArgumentsAsFlagsWithExplicitValues()
    {
        Parse<ModelWithConstructor<bool?>>("--first", "true", "--third", "false")
            .ShouldSucceed(new ModelWithConstructor<bool?>(true, null, false));

        Parse<ModelWithConstructor<bool?>>("--first", "TRUE", "--third", "FALSE")
            .ShouldSucceed(new ModelWithConstructor<bool?>(true, null, false));

        Parse<ModelWithConstructor<bool?>>("--first", "on", "--third", "off")
            .ShouldSucceed(new ModelWithConstructor<bool?>(true, null, false));

        Parse<ModelWithConstructor<bool?>>("--first", "ON", "--third", "OFF")
            .ShouldSucceed(new ModelWithConstructor<bool?>(true, null, false));

        Parse<ModelWithConstructor<bool?>>("--first", "value1", "--third", "value2")
            .ShouldFail("--first was not in the correct format.");

        //Unspecified params[]
        Parse<ModelWithParams<bool?>>("--first", "true", "--third", "false")
            .ShouldSucceed(new ModelWithParams<bool?>(true, null, false));

        Parse<ModelWithParams<bool?>>("--first", "TRUE", "--third", "FALSE")
            .ShouldSucceed(new ModelWithParams<bool?>(true, null, false));

        Parse<ModelWithParams<bool?>>("--first", "on", "--third", "off")
            .ShouldSucceed(new ModelWithParams<bool?>(true, null, false));

        Parse<ModelWithParams<bool?>>("--first", "ON", "--third", "OFF")
            .ShouldSucceed(new ModelWithParams<bool?>(true, null, false));

        Parse<ModelWithParams<bool?>>("--first", "value1", "--third", "value2")
            .ShouldFail("--first was not in the correct format.");

        //Specified params[]
        Parse<ModelWithParams<bool?>>("--first", "true", "--third", "false", "true", "false")
            .ShouldSucceed(new ModelWithParams<bool?>(true, null, false, true, false));

        Parse<ModelWithParams<bool?>>("--first", "TRUE", "--third", "FALSE", "FALSE", "TRUE")
            .ShouldSucceed(new ModelWithParams<bool?>(true, null, false, false, true));

        Parse<ModelWithParams<bool?>>("--first", "on", "--third", "off", "on", "off")
            .ShouldSucceed(new ModelWithParams<bool?>(true, null, false, true, false));

        Parse<ModelWithParams<bool?>>("--first", "ON", "--third", "OFF", "OFF", "ON")
            .ShouldSucceed(new ModelWithParams<bool?>(true, null, false, false, true));

        Parse<ModelWithParams<bool?>>("--first", "true", "--third", "false", "value")
            .ShouldFail("rest was not in the correct format.");
    }

    public void ShouldParseEnumArgumentsByTheirCaseInsensitiveStringRepresentation()
    {
        Parse<ModelWithConstructor<Level>>("--first", "Warning", "--third", "ErRoR")
            .ShouldSucceed(new ModelWithConstructor<Level>(Level.Warning, Level.Information, Level.Error));

        Parse<ModelWithConstructor<Level>>("--first", "Warning", "--third", "TYPO")
            .ShouldFail("--third must be one of: Information, Warning, Error.");

        Parse<ModelWithConstructor<Level?>>("--first", "Warning", "--third", "eRrOr")
            .ShouldSucceed(new ModelWithConstructor<Level?>(Level.Warning, null, Level.Error));

        Parse<ModelWithConstructor<Level?>>("--first", "Warning", "--third", "TYPO")
            .ShouldFail("--third must be one of: Information, Warning, Error.");

        //Unspecified params[]
        Parse<ModelWithParams<Level>>("--first", "Warning", "--third", "ErRoR")
            .ShouldSucceed(new ModelWithParams<Level>(Level.Warning, Level.Information, Level.Error));

        Parse<ModelWithParams<Level>>("--first", "Warning", "--third", "TYPO")
            .ShouldFail("--third must be one of: Information, Warning, Error.");

        Parse<ModelWithParams<Level?>>("--first", "Warning", "--third", "eRrOr")
            .ShouldSucceed(new ModelWithParams<Level?>(Level.Warning, null, Level.Error));

        Parse<ModelWithParams<Level?>>("--first", "Warning", "--third", "TYPO")
            .ShouldFail("--third must be one of: Information, Warning, Error.");

        //Specified params[]
        Parse<ModelWithParams<Level>>("--first", "Warning", "--third", "ErRoR", "Error")
            .ShouldSucceed(new ModelWithParams<Level>(Level.Warning, Level.Information, Level.Error, Level.Error));

        Parse<ModelWithParams<Level>>("--first", "Warning", "--third", "ErRoR", "TYPO")
            .ShouldFail("rest must be one of: Information, Warning, Error.");

        Parse<ModelWithParams<Level?>>("--first", "Warning", "--third", "ErRoR", "eRrOr")
            .ShouldSucceed(new ModelWithParams<Level?>(Level.Warning, null, Level.Error, Level.Error));

        Parse<ModelWithParams<Level?>>("--first", "Warning", "--third", "ErRoR", "eRrOr", "TYPO")
            .ShouldFail("rest must be one of: Information, Warning, Error.");
    }

    public void ShouldFailForUnexpectedArguments()
    {
        Parse<Empty>("first", "second")
            .ShouldFail("Unexpected argument: first");

        Parse<ModelWithConstructor<string>>(
            "--first", "value1",
            "--second", "value2",
            "--third", "value3",
            "--fourth", "value4")
            .ShouldFail("Unexpected argument: --fourth");
        
        Parse<ModelWithConstructor<string>>(
            "--first", "value1",
            "--second", "value2",
            "--third", "value3",
            "value4")
            .ShouldFail("Unexpected argument: value4");

        Parse<ModelWithConstructor<string>>(
                "--first", "value1",
                "--second", "value2",
                "--third", "value3",
                "-x", "value4")
            .ShouldFail("Unexpected argument: -x");
    }

    public void ShouldFailWhenNonArrayArgumentsAreRepeated()
    {
        Parse<ModelWithConstructor<int>>("--first", "1", "--second", "2", "-f", "3")
            .ShouldFail("--first cannot be specified more than once.");
    }

    class ModelWithArrays
    {
        public ModelWithArrays(int[] integer, string[] @string)
        {
            Integer = integer;
            String = @string;
        }

        public int[] Integer { get; }
        public string[] String { get; }
    }

    public void ShouldParseRepeatedArgumentsAsArrayItems()
    {
        Parse<ModelWithArrays>(
            "--integer", "1", "--integer", "2",
            "--string", "three", "--string", "four")
            .ShouldSucceed(new ModelWithArrays([1, 2], ["three", "four"]));
    }

    public void ShouldSetEmptyArraysForMissingArrayArguments()
    {
        Parse<ModelWithArrays>()
            .ShouldSucceed(new ModelWithArrays([], []));
    }

    class AmbiguousParameters
    {
        public AmbiguousParameters(int PARAMETER, int parameter)
        {
            this.PARAMETER = PARAMETER;
            this.parameter = parameter;
        }

        public int PARAMETER { get; }
        public int parameter { get; }
    }

    class AmbiguousFirstLetterParameters
    {
        public AmbiguousFirstLetterParameters(
            string pattern,
            string configuration,
            string password,
            string convention,
            string prefix)
        {
            Pattern = pattern;
            Configuration = configuration;
            Password = password;
            Convention = convention;
            Prefix = prefix;
        }

        public string Pattern { get; }
        public string Configuration { get; }
        public string Password { get; }
        public string Convention { get; }
        public string Prefix { get; } 
    }

    public void ShouldDemandUnambiguousParameterNames()
    {
        Parse<AmbiguousParameters>()
            .ShouldFail("Parsing command line arguments for type AmbiguousParameters " +
                        "is ambiguous, because it has more than one parameter corresponding " +
                        "with the --parameter argument.");
    }

    public void ShouldDemandUnambiguousFirstLetterParameterNameAbbreviations()
    {
        Parse<AmbiguousFirstLetterParameters>("-p", "a*c", "-c")
            .ShouldFail("-p is not a recognized option. Did you mean --password, --pattern, or --prefix?");

        Parse<AmbiguousFirstLetterParameters>("--pattern", "a*c", "-c")
            .ShouldFail("-c is not a recognized option. Did you mean --configuration or --convention?");
    }

    class Complex
    {
        public Complex(
            string? @string,
            int integer,
            bool @bool,
            int? nullableInteger,
            bool? nullableBoolean,
            string[] strings,
            int[] integers)
        {
            String = @string;
            Integer = integer;
            Bool = @bool;
            NullableInteger = nullableInteger;
            NullableBoolean = nullableBoolean;
            Strings = strings;
            Integers = integers;
        }

        public string? String { get; }
        public int Integer { get; }
        public bool Bool { get; }
        public int? NullableInteger { get; }
        public bool? NullableBoolean { get; }
        public string[] Strings { get; }
        public int[] Integers { get; }
    }

    public void ShouldBindCommandLineArgumentsToComplexModels()
    {
        Parse<Complex>()
            .ShouldSucceed(new Complex(null, 0, false, null, null, [], []));

        Parse<Complex>(
            "--string", "def",
            "--integer", "34",
            "--bool",
            "--nullable-integer", "56",
            "--nullable-boolean", "off",
            "--strings", "first",
            "--strings", "second",
            "--integers", "78",
            "--integers", "90")
            .ShouldSucceed(new Complex(
                    "def", 34, true, 56, false,
                    ["first", "second"],
                    [78, 90]));
    }

    class ComplexWithParams
    {
        public ComplexWithParams(
            string? @string,
            int integer,
            bool @bool,
            int? nullableInteger,
            bool? nullableBoolean,
            string[] strings,
            int[] integers,
            params string[] rest)
        {
                String = @string;
                Integer = integer;
                Bool = @bool;
                NullableInteger = nullableInteger;
                NullableBoolean = nullableBoolean;
                Strings = strings;
                Integers = integers;
                Rest = rest;
            }

        public string? String { get; }
        public int Integer { get; }
        public bool Bool { get; }
        public int? NullableInteger { get; }
        public bool? NullableBoolean { get; }
        public string[] Strings { get; }
        public int[] Integers { get; }
        public string[] Rest { get; }
    }

    public void ShouldBindCommandLineArgumentsToComplexModelsWithParams()
    {
        Parse<ComplexWithParams>()
            .ShouldSucceed(new ComplexWithParams(null, 0, false, null, null, [], []));

        Parse<ComplexWithParams>(
            "--string", "def",
            "--integer", "34",
            "--bool",
            "--nullable-integer", "56",
            "--nullable-boolean", "off",
            "--strings", "first",
            "--strings", "second",
            "positionalArgumentA",
            "--integers", "78",
            "positionalArgumentB",
            "--integers", "90",
            "positionalArgumentC")
            .ShouldSucceed(new ComplexWithParams(
                    "def", 34, true, 56, false,
                    ["first", "second"],
                    [78, 90],
                    "positionalArgumentA", "positionalArgumentB", "positionalArgumentC"));
    }

    static Scenario<T> Parse<T>(params string[] arguments) where T : class
    {
        return new Scenario<T>(arguments);
    }

    class Scenario<T> where T : class
    {
        readonly string[] arguments;

        public Scenario(params string[] arguments)
        {
            this.arguments = arguments;
        }

        public void ShouldSucceed(T expectedModel)
        {
            var model = CommandLine.Parse<T>(arguments);
            model.ShouldMatch(expectedModel);
        }

        public void ShouldFail(string expectedExceptionMessage)
        {
            var shouldThrow = () => CommandLine.Parse<T>(arguments);

            shouldThrow.ShouldThrow<CommandLineException>(expectedExceptionMessage);
        }
    }
}