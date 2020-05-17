namespace Fixie.Tests.Cli
{
    using System;
    using Assertions;
    using Fixie.Cli;

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

            Parse<ModelWithConstructor<int>>("--first", "1", "--second", "--third", "3")
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

            //Unspecified params[]
            Parse<ModelWithConstructor<bool>>("--first", "--third")
                .ShouldSucceed(new ModelWithConstructor<bool>(true, false, true));

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

        public void ShouldCollectExcessArgumentsForLaterInspection()
        {
            Parse<Empty>("first", "second", "third", "fourth", "fifth")
                .ShouldSucceed(new Empty(), "first", "second", "third", "fourth", "fifth");

            Parse<ModelWithConstructor<string>>(
                "--first", "value1",
                "--second", "value2",
                "--third", "value3",
                "--fourth", "value4",
                "--array", "value5",
                "--array", "--value6",
                "value7")
                .ShouldSucceed(new ModelWithConstructor<string>("value1", "value2", "value3"),
                    "--fourth", "value4", "--array", "value5", "--array", "--value6", "value7");

            Parse<ModelWithConstructor<int>>(
                "--first", "1",
                "--second", "2",
                "--third", "3",
                "--fourth", "4",
                "--array", "5",
                "--array", "6",
                "7")
                .ShouldSucceed(new ModelWithConstructor<int>(1, 2, 3),
                    "--fourth", "4", "--array", "5", "--array", "6", "7");

            //Excess unnamed arguments are taken by a params[] array, when one is declared.

            Parse<ModelWithParams<string>>(
                    "--first", "value1",
                    "--second", "value2",
                    "--third", "value3",
                    "--fourth", "value4",
                    "--array", "value5",
                    "--array", "value6",
                    "value7")
                .ShouldSucceed(new ModelWithParams<string>("value1", "value2", "value3", "value7"),
                    "--fourth", "value4", "--array", "value5", "--array", "value6");

            Parse<ModelWithParams<int>>(
                    "--first", "1",
                    "--second", "2",
                    "--third", "3",
                    "--fourth", "4",
                    "--array", "5",
                    "--array", "6",
                    "7")
                .ShouldSucceed(new ModelWithParams<int>(1, 2, 3, 7),
                    "--fourth", "4", "--array", "5", "--array", "6");

            Parse<ModelWithParams<int>>(
                    "--first", "1",
                    "--second", "2",
                    "--third", "3",
                    "--fourth", "4",
                    "--array", "5",
                    "--array", "6",
                    "value7")
                .ShouldFail("rest was not in the correct format.");
        }

        public void ShouldFailWhenNonArrayArgumentsAreRepeated()
        {
            Parse<ModelWithConstructor<int>>("--first", "1", "--second", "2", "--first", "3")
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
                .ShouldSucceed(new ModelWithArrays(new[] { 1, 2 }, new[] { "three", "four" }));
        }

        public void ShouldSetEmptyArraysForMissingArrayArguments()
        {
            Parse<ModelWithArrays>()
                .ShouldSucceed(new ModelWithArrays(new int[] { }, new string[] { }));
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

        public void ShouldDemandUnambiguousParameterNames()
        {
            Parse<AmbiguousParameters>()
                .ShouldFail("Parsing command line arguments for type AmbiguousParameters " +
                            "is ambiguous, because it has more than one parameter corresponding " +
                            "with the --parameter argument.");
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
                .ShouldSucceed(new Complex(null, 0, false, null, null,
                    new string[] { }, new int[] { }));

            Parse<Complex>(
                "--string", "def",
                "--integer", "34",
                "--bool",
                "--nullable-integer", "56",
                "--nullable-boolean", "off",
                "--strings", "first",
                "--unexpected-argument",
                "--strings", "second",
                "unexpectedArgument",
                "--integers", "78",
                "--unexpected-argument-with-value", "unexpectedValue",
                "--integers", "90")
                .ShouldSucceed(new Complex(
                        "def", 34, true, 56, false,
                        new[] { "first", "second" },
                        new[] { 78, 90 }),
                    expectedUnusedArguments: new[]
                    {
                        "--unexpected-argument",
                        "unexpectedArgument",
                        "--unexpected-argument-with-value",
                        "unexpectedValue"
                    });
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
                .ShouldSucceed(new ComplexWithParams(null, 0, false, null, null,
                    new string[] { }, new int[] { }));

            //Here, "unexpectedArgument" is claimed as part of our params[] array,
            //so only unexpected named arguments are truly unused by the parser.
            Parse<ComplexWithParams>(
                "--string", "def",
                "--integer", "34",
                "--bool",
                "--nullable-integer", "56",
                "--nullable-boolean", "off",
                "--strings", "first",
                "--unexpected-argument",
                "--strings", "second",
                "unexpectedArgument",
                "--integers", "78",
                "--unexpected-argument-with-value", "unexpectedValue",
                "--integers", "90")
                .ShouldSucceed(new ComplexWithParams(
                        "def", 34, true, 56, false,
                        new[] { "first", "second" },
                        new[] { 78, 90 },
                        "unexpectedArgument"),
                    expectedUnusedArguments: new[]
                    {
                        "--unexpected-argument",
                        "--unexpected-argument-with-value",
                        "unexpectedValue"
                    });
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

            public void ShouldSucceed(T expectedModel, params string[] expectedUnusedArguments)
            {
                string[] unusedArguments;
                var model = CommandLine.Parse<T>(arguments, out unusedArguments);
                ShouldMatch(model, expectedModel);
                unusedArguments.ShouldBe(expectedUnusedArguments);
            }

            public void ShouldFail(string expectedExceptionMessage)
            {
                Action shouldThrow = () => CommandLine.Parse<T>(arguments);

                shouldThrow.ShouldThrow<CommandLineException>(expectedExceptionMessage);
            }

            static void ShouldMatch(T actual, T expected)
            {
                foreach (var property in typeof(T).GetProperties())
                {
                    var actualValue = property.GetValue(actual);
                    var expectedValue = property.GetValue(expected);

                    actualValue.ShouldBe(expectedValue);
                }
            }
        }
    }
}