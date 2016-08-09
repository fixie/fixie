namespace Fixie.Tests.Cli
{
    using System;
    using Fixie.Cli;
    using Should;

    public class CommandLineTests
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
                .ShouldFail("Parsing command line arguments for type TooManyConstructors " +
                            "is ambiguous, because it has more than one constructor.");
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

        public void ShouldParseArgumentsAsConstructorParameters()
        {
            Parse<ModelWithConstructor<string>>("first", "second", "third")
                .ShouldSucceed(new ModelWithConstructor<string>("first", "second", "third"));

            Parse<ModelWithConstructor<int>>("1", "2", "3")
                .ShouldSucceed(new ModelWithConstructor<int>(1, 2, 3));

            Parse<ModelWithConstructor<int>>("1", "2", "abc")
                .ShouldFail("third was not in the correct format.");
        }

        public void ShouldPassDefaultValuesToMissingConstructorParameters()
        {
            Parse<ModelWithConstructor<string>>("first", "second")
                .ShouldSucceed(new ModelWithConstructor<string>("first", "second", null));

            Parse<ModelWithConstructor<int>>("1", "2")
                .ShouldSucceed(new ModelWithConstructor<int>(1, 2, 0));
        }

        public void ShouldParseNullableValueTypeArguments()
        {
            Parse<ModelWithConstructor<int?>>("1", "2")
                .ShouldSucceed(new ModelWithConstructor<int?>(1, 2, null));

            Parse<ModelWithConstructor<char?>>("a", "b")
                .ShouldSucceed(new ModelWithConstructor<char?>('a', 'b', null));
        }

        public void ShouldParseBoolArgumentsWithExplicitValues()
        {
            Parse<ModelWithConstructor<bool>>("true", "false")
                .ShouldSucceed(new ModelWithConstructor<bool>(true, false, false));

            Parse<ModelWithConstructor<bool>>("on", "off")
                .ShouldSucceed(new ModelWithConstructor<bool>(true, false, false));

            Parse<ModelWithConstructor<bool>>("value1", "value2")
                .ShouldFail("first was not in the correct format.");
        }

        public void ShouldParseNullableBoolArgumentsWithExplicitValues()
        {
            Parse<ModelWithConstructor<bool?>>("true", "false")
                .ShouldSucceed(new ModelWithConstructor<bool?>(true, false, null));

            Parse<ModelWithConstructor<bool?>>("on", "off")
                .ShouldSucceed(new ModelWithConstructor<bool?>(true, false, null));

            Parse<ModelWithConstructor<bool?>>("value1", "value2")
                .ShouldFail("first was not in the correct format.");
        }

        public void ShouldParseEnumArgumentsByTheirCaseInsensitiveStringRepresentation()
        {
            Parse<ModelWithConstructor<Level>>("Warning", "ErRoR")
                .ShouldSucceed(new ModelWithConstructor<Level>(Level.Warning, Level.Error, Level.Information));

            Parse<ModelWithConstructor<Level>>("Warning", "TYPO")
                .ShouldFail("second must be one of: Information, Warning, Error.");

            Parse<ModelWithConstructor<Level?>>("Warning", "eRrOr")
                .ShouldSucceed(new ModelWithConstructor<Level?>(Level.Warning, Level.Error, null));

            Parse<ModelWithConstructor<Level?>>("Warning", "TYPO")
                .ShouldFail("second must be one of: Information, Warning, Error.");
        }

        public void ShouldCollectExcessArgumentsForLaterInspection()
        {
            Parse<ModelWithConstructor<string>>("first", "second", "third", "fourth", "fifth")
                .ShouldSucceed(
                    new ModelWithConstructor<string>("first", "second", "third"),
                    "fourth", "fifth");

            Parse<ModelWithConstructor<int>>("1", "2", "3", "4", "5")
                .ShouldSucceed(
                    new ModelWithConstructor<int>(1, 2, 3),
                    "4", "5");
        }

        class ModelWithProperties<T>
        {
            public T First { get; set; }
            public T Second { get; set; }
            public T Third { get; set; }
        }

        public void ShouldParseNamedArgumentsAsProperties()
        {
            Parse<ModelWithProperties<string>>("--first", "value1", "--second", "value2", "--third", "value3")
                .ShouldSucceed(new ModelWithProperties<string>
                {
                    First = "value1", Second = "value2", Third = "value3"
                });

            Parse<ModelWithProperties<int>>("--first", "1", "--second", "2", "--third", "3")
                .ShouldSucceed(new ModelWithProperties<int>
                {
                    First = 1, Second = 2, Third = 3
                });

            Parse<ModelWithProperties<int>>("--first", "1", "--second", "2", "--third", "abc")
                .ShouldFail("--third was not in the correct format.");
        }

        public void ShouldFailWhenNamedArgumentsAreMissingTheirRequiredValues()
        {
            Parse<ModelWithProperties<int>>("--first", "1", "--second", "2", "--third")
                .ShouldFail("--third is missing its required value.");

            Parse<ModelWithProperties<int>>("--first", "1", "--second", "--third", "3")
                .ShouldFail("--second is missing its required value.");
        }

        public void ShouldLeaveDefaultValuesForMissingNamedArguments()
        {
            Parse<ModelWithProperties<string>>("--first", "value1", "--second", "value2")
                .ShouldSucceed(new ModelWithProperties<string>
                {
                    First = "value1", Second = "value2", Third = null
                });

            Parse<ModelWithProperties<int>>("--first", "1", "--second", "2")
                .ShouldSucceed(new ModelWithProperties<int>
                {
                    First = 1, Second = 2, Third = 0
                });
        }

        public void ShouldParseNullableValueTypeNamedArguments()
        {
            Parse<ModelWithProperties<int?>>("--first", "1", "--third", "2")
                .ShouldSucceed(new ModelWithProperties<int?>
                {
                    First = 1, Second = null, Third = 2
                });

            Parse<ModelWithProperties<char?>>("--first", "a", "--third", "c")
                .ShouldSucceed(new ModelWithProperties<char?>
                {
                    First = 'a', Second = null, Third = 'c'
                });
        }

        public void ShouldParseBoolNamedArgumentsAsFlagsWithoutExplicitValues()
        {
            Parse<ModelWithProperties<bool>>("--first", "--third")
                .ShouldSucceed(new ModelWithProperties<bool>
                {
                    First = true, Second = false, Third = true
                });
        }

        public void ShouldParseNullableBoolNamedArgumentsAsFlagsWithExplicitValues()
        {
            Parse<ModelWithProperties<bool?>>("--first", "true", "--third", "false")
                .ShouldSucceed(new ModelWithProperties<bool?>
                {
                    First = true, Second = null, Third = false
                });

            Parse<ModelWithProperties<bool?>>("--first", "on", "--third", "off")
                .ShouldSucceed(new ModelWithProperties<bool?>
                {
                    First = true, Second = null, Third = false
                });

            Parse<ModelWithProperties<bool?>>("--first", "value1", "--third", "value2")
                .ShouldFail("--first was not in the correct format.");
        }

        public void ShouldParseEnumNamedArgumentsByTheirCaseInsensitiveStringRepresentation()
        {
            Parse<ModelWithProperties<Level>>("--first", "Warning", "--third", "ErRoR")
                .ShouldSucceed(new ModelWithProperties<Level>
                {
                    First = Level.Warning, Second = Level.Information, Third = Level.Error
                });

            Parse<ModelWithProperties<Level>>("--first", "Warning", "--third", "TYPO")
                .ShouldFail("--third must be one of: Information, Warning, Error.");

            Parse<ModelWithProperties<Level?>>("--first", "Warning", "--third", "eRrOr")
                .ShouldSucceed(new ModelWithProperties<Level?>
                {
                    First = Level.Warning, Second = null, Third = Level.Error
                });

            Parse<ModelWithProperties<Level?>>("--first", "Warning", "--third", "TYPO")
                .ShouldFail("--third must be one of: Information, Warning, Error.");
        }

        public void ShouldCollectExcessNamedArgumentsForLaterInspection()
        {
            Parse<ModelWithProperties<string>>(
                "--first", "value1",
                "--second", "value2",
                "--third", "value3",
                "--fourth", "value4",
                "--array", "value5",
                "--array", "--value6")
                .ShouldSucceed(new ModelWithProperties<string>
                {
                    First = "value1", Second = "value2", Third = "value3"
                },
                    "--fourth", "value4", "--array", "value5", "--array", "--value6");

            Parse<ModelWithProperties<int>>(
                "--first", "1",
                "--second", "2",
                "--third", "3",
                "--fourth", "4",
                "--array", "5",
                "--array", "6")
                .ShouldSucceed(new ModelWithProperties<int>
                {
                    First = 1, Second = 2, Third = 3
                },
                    "--fourth", "4", "--array", "5", "--array", "6");
        }

        public void ShouldFailWhenNonArrayNamedArgumentsAreRepeated()
        {
            Parse<ModelWithProperties<int>>("--first", "1", "--second", "2", "--first", "3")
                .ShouldFail("--first cannot be specified more than once.");
        }

        class ModelWithArrays
        {
            public int[] Integer { get; set; }
            public string[] String { get; set; }
        }

        public void ShouldParseRepeatedNamedArgumentsAsArrayProperties()
        {
            Parse<ModelWithArrays>(
                "--integer", "1", "--integer", "2",
                "--string", "three", "--string", "four")
                .ShouldSucceed(new ModelWithArrays
                {
                    Integer = new[] { 1, 2 },
                    String = new[] { "three", "four" }
                });
        }

        public void ShouldSetEmptyArraysForMissingArrayNamedArguments()
        {
            Parse<ModelWithArrays>()
                .ShouldSucceed(new ModelWithArrays
                {
                    Integer = new int[] { },
                    String = new string[] { }
                });
        }

        class AmbiguousProperties
        {
            public int PROPERTY { get; set; }
            public int property { get; set; }
        }

        public void ShouldDemandUnambiguousPropertyNames()
        {
            Parse<AmbiguousProperties>()
                .ShouldFail("Parsing command line arguments for type AmbiguousProperties " +
                            "is ambiguous, because it has more than one property corresponding " +
                            "with the --property argument.");
        }

        class Complex
        {
            public Complex(string firstArgument, int secondArgument)
            {
                FirstArgument = firstArgument;
                SecondArgument = secondArgument;
            }

            public string FirstArgument { get; }
            public int SecondArgument { get; }

            public string String { get; set; }
            public int Integer { get; set; }
            public bool Bool { get; set; }
            public int? NullableInteger { get; set; }
            public bool? NullableBoolean { get; set; }

            public string[] Strings { get; set; }
            public int[] Integers { get; set; }
        }

        public void ShouldBindCommandLineArgumentsToComplexModels()
        {
            Parse<Complex>()
                .ShouldSucceed(new Complex(null, 0)
                {
                    Strings = new string[] { },
                    Integers = new int[] { }
                });

            Parse<Complex>("--string", "def", "abc", "12",
                "--integer", "34",
                "--bool",
                "--nullable-integer", "56",
                "--nullable-boolean", "off",
                "--strings", "first",
                "--unexpected-option",
                "--strings", "second",
                "unexpectedArgument",
                "--integers", "78",
                "--unexpected-option-with-value", "unexpectedValue",
                "--integers", "90")
                .ShouldSucceed(new Complex("abc", 12)
                {
                    String = "def",
                    Integer = 34,
                    Bool = true,
                    NullableInteger = 56,
                    NullableBoolean = false,
                    Strings = new[] { "first", "second" },
                    Integers = new[] { 78, 90 }
                }, expectedUnusedArguments: new[]
                    {
                        "--unexpected-option",
                        "unexpectedArgument",
                        "--unexpected-option-with-value",
                        "unexpectedValue"
                    });
        }

        static Scenario<T> Parse<T>(params string[] args) where T : class
        {
            return new Scenario<T>(args);
        }

        class Scenario<T> where T : class
        {
            readonly string[] args;

            public Scenario(params string[] args)
            {
                this.args = args;
            }

            public void ShouldSucceed(T expectedModel, params string[] expectedUnusedArguments)
            {
                string[] unusedArguments;
                var model = CommandLine.Parse<T>(args, out unusedArguments);
                model.ShouldMatch(expectedModel);
                unusedArguments.ShouldEqual(expectedUnusedArguments);
            }

            public void ShouldFail(string expectedExceptionMessage)
            {
                Action shouldThrow = () => CommandLine.Parse<T>(args);

                shouldThrow.ShouldThrow<CommandLineException>(expectedExceptionMessage);
            }
        }
    }

    public static class AssertionExtensions
    {
        public static void ShouldMatch<T>(this T actual, T expected)
        {
            foreach (var property in typeof(T).GetProperties())
            {
                var actualValue = property.GetValue(actual);
                var expectedValue = property.GetValue(expected);

                actualValue.ShouldEqual(expectedValue);
            }
        }
    }
}
