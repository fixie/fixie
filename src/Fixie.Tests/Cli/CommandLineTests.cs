namespace Fixie.Tests.Cli
{
    using System;
    using Fixie.Cli;
    using Should;

    public class CommandLineTests
    {
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
            Action tooManyConstructors = () => Parse<TooManyConstructors>();

            tooManyConstructors.ShouldThrow<Exception>(
                "Parsing command line arguments for type TooManyConstructors " +
                "is ambiguous, because it has more than one constructor.");
        }

        class ModelWithArguments<T>
        {
            public T First { get; }
            public T Second { get; }
            public T Third { get; }

            public ModelWithArguments(T first, T second, T third)
            {
                First = first;
                Second = second;
                Third = third;
            }
        }

        public void ShouldParseArgumentsAsConstructorParameters()
        {
            Parse<ModelWithArguments<string>>("first", "second", "third")
                .ShouldSucceed(new ModelWithArguments<string>("first", "second", "third"));

            Parse<ModelWithArguments<int>>("1", "2", "3")
                .ShouldSucceed(new ModelWithArguments<int>(1, 2, 3));
        }

        public void ShouldPassDefaultValuesToMissingConstructorParameters()
        {
            Parse<ModelWithArguments<string>>("first", "second")
                .ShouldSucceed(new ModelWithArguments<string>("first", "second", null));

            Parse<ModelWithArguments<int>>("1", "2")
                .ShouldSucceed(new ModelWithArguments<int>(1, 2, 0));
        }

        public void ShouldParseNullableValueTypeArguments()
        {
            Parse<ModelWithArguments<int?>>("1", "2")
                .ShouldSucceed(new ModelWithArguments<int?>(1, 2, null));

            Parse<ModelWithArguments<char?>>("a", "b")
                .ShouldSucceed(new ModelWithArguments<char?>('a', 'b', null));
        }

        public void ShouldParseBoolArgumentsWithExplicitValues()
        {
            Parse<ModelWithArguments<bool>>("true", "false")
                .ShouldSucceed(new ModelWithArguments<bool>(true, false, false));

            Parse<ModelWithArguments<bool>>("on", "off")
                .ShouldSucceed(new ModelWithArguments<bool>(true, false, false));

            Parse<ModelWithArguments<bool>>("value1", "value2")
                .ShouldFail(new ModelWithArguments<bool>(false, false, false),
                    "Expected first to be convertible to bool.",
                    "Expected second to be convertible to bool.");
        }

        public void ShouldParseNullableBoolArgumentsWithExplicitValues()
        {
            Parse<ModelWithArguments<bool?>>("true", "false")
                .ShouldSucceed(new ModelWithArguments<bool?>(true, false, null));

            Parse<ModelWithArguments<bool?>>("on", "off")
                .ShouldSucceed(new ModelWithArguments<bool?>(true, false, null));

            Parse<ModelWithArguments<bool?>>("value1", "value2")
                .ShouldFail(new ModelWithArguments<bool?>(null, null, null),
                    "Expected first to be convertible to bool?.",
                    "Expected second to be convertible to bool?.");
        }

        public void ShouldFailWithClearExplanationWhenArgumentsAreNotConvertibleToParameterTypes()
        {
            Parse<ModelWithArguments<int>>("1", "2", "abc")
                .ShouldFail(new ModelWithArguments<int>(1, 2, 0),
                    "Expected third to be convertible to int.");
        }

        public void ShouldCollectExcessArgumentsForLaterInspection()
        {
            Parse<ModelWithArguments<string>>("first", "second", "third", "fourth", "fifth")
                .ShouldSucceed(
                    new ModelWithArguments<string>("first", "second", "third"),
                    "fourth", "fifth");

            Parse<ModelWithArguments<int>>("1", "2", "3", "4", "5")
                .ShouldSucceed(
                    new ModelWithArguments<int>(1, 2, 3),
                    "4", "5");
        }

        class ModelWithOptions<T>
        {
            public T First { get; set; }
            public T Second { get; set; }
            public T Third { get; set; }
        }

        public void ShouldParseOptionsAsProperties()
        {
            Parse<ModelWithOptions<string>>("--first", "value1", "--second", "value2", "--third", "value3")
                .ShouldSucceed(new ModelWithOptions<string>
                {
                    First = "value1", Second = "value2", Third = "value3"
                });

            Parse<ModelWithOptions<int>>("--first", "1", "--second", "2", "--third", "3")
                .ShouldSucceed(new ModelWithOptions<int>
                {
                    First = 1, Second = 2, Third = 3
                });
        }

        public void ShouldLeaveDefaultValuesForMissingOptions()
        {
            Parse<ModelWithOptions<string>>("--first", "value1", "--second", "value2")
                .ShouldSucceed(new ModelWithOptions<string>
                {
                    First = "value1", Second = "value2", Third = null
                });

            Parse<ModelWithOptions<int>>("--first", "1", "--second", "2")
                .ShouldSucceed(new ModelWithOptions<int>
                {
                    First = 1, Second = 2, Third = 0
                });
        }

        public void ShouldFailWithClearExplanationWhenOptionsAreNotConvertibleToPropertyTypes()
        {
            Parse<ModelWithOptions<int>>("--first", "1", "--second", "2", "--third", "abc")
                .ShouldFail(new ModelWithOptions<int>
                {
                    First = 1, Second = 2, Third = 0
                },
                "Expected --third to be convertible to int.");
        }

        public void ShouldFailWithClearExplanationWhenOptionsAreMissingTheirRequiredValues()
        {
            Parse<ModelWithOptions<int>>("--first", "1", "--second", "2", "--third")
                .ShouldFail(new ModelWithOptions<int>
                {
                    First = 1, Second = 2, Third = 0
                },
                "Option --third is missing its required value.");

            Parse<ModelWithOptions<int>>("--first", "1", "--second", "--third", "3")
                .ShouldFail(new ModelWithOptions<int>
                {
                    First = 1, Second = 0, Third = 3
                },
                "Option --second is missing its required value.");
        }

        public void ShouldParseNullableValueTypeOptions()
        {
            Parse<ModelWithOptions<int?>>("--first", "1", "--third", "2")
                .ShouldSucceed(new ModelWithOptions<int?>
                {
                    First = 1, Second = null, Third = 2
                });

            Parse<ModelWithOptions<char?>>("--first", "a", "--third", "c")
                .ShouldSucceed(new ModelWithOptions<char?>
                {
                    First = 'a', Second = null, Third = 'c'
                });
        }

        public void ShouldParseBoolOptionsAsFlagsWithoutExplicitValues()
        {
            Parse<ModelWithOptions<bool>>("--first", "--third")
                .ShouldSucceed(new ModelWithOptions<bool>
                {
                    First = true, Second = false, Third = true
                });
        }

        public void ShouldParseNullableBoolOptionsAsFlagsWithExplicitValues()
        {
            Parse<ModelWithOptions<bool?>>("--first", "true", "--third", "false")
                .ShouldSucceed(new ModelWithOptions<bool?>
                {
                    First = true, Second = null, Third = false
                });

            Parse<ModelWithOptions<bool?>>("--first", "on", "--third", "off")
                .ShouldSucceed(new ModelWithOptions<bool?>
                {
                    First = true, Second = null, Third = false
                });

            Parse<ModelWithOptions<bool?>>("--first", "value1", "--third", "value2")
                .ShouldFail(new ModelWithOptions<bool?>
                {
                    First = null, Second = null, Third = null
                },
                "Expected --first to be convertible to bool?.",
                "Expected --third to be convertible to bool?.");
        }

        public void ShouldFailWithClearExplanationWhenNonArrayOptionsAreRepeated()
        {
            Parse<ModelWithOptions<int>>("--first", "1", "--second", "2", "--first", "3")
                .ShouldFail(new ModelWithOptions<int>
                {
                    First = 1,
                    Second = 2,
                    Third = 0
                },
                "Option --first cannot be specified more than once.");
        }

        class ModelWithArrays
        {
            public int[] Integer { get; set; }
            public string[] String { get; set; }
        }

        public void ShouldParseRepeatedOptionsAsArrayProperties()
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

        public void ShouldSetEmptyArraysForMissingArrayOptions()
        {
            Parse<ModelWithArrays>()
                .ShouldSucceed(new ModelWithArrays
                {
                    Integer = new int[] { },
                    String = new string[] { }
                });
        }

        public void ShouldCollectExcessOptionsForLaterInspection()
        {
            Parse<ModelWithOptions<string>>(
                "--first", "value1",
                "--second", "value2",
                "--third", "value3",
                "--fourth", "value4",
                "--array", "value5",
                "--array", "--value6")
                .ShouldSucceed(new ModelWithOptions<string>
                {
                    First = "value1",
                    Second = "value2",
                    Third = "value3"
                },
                "--fourth", "value4", "--array", "value5", "--array", "--value6");

            Parse<ModelWithOptions<int>>(
                "--first", "1",
                "--second", "2",
                "--third", "3",
                "--fourth", "4",
                "--array", "5",
                "--array", "6")
                .ShouldSucceed(new ModelWithOptions<int>
                {
                    First = 1,
                    Second = 2,
                    Third = 3
                },
                "--fourth", "4", "--array", "5", "--array", "6");
        }

        class AmbiguousOptions
        {
            public int PROPERTY { get; set; }
            public int property { get; set; }
        }

        public void ShouldDemandUnambiguousPropertyNames()
        {
            Action ambiguousPropertyNames = () => Parse<AmbiguousOptions>();

            ambiguousPropertyNames.ShouldThrow<Exception>(
                "Parsing command line arguments for type AmbiguousOptions " +
                "is ambiguous, because it has more than one property corresponding " +
                "with the --property option.");
        }
        static Scenario<T> Parse<T>(params string[] args) where T : class
        {
            return new Scenario<T>(args);
        }

        class Scenario<T> where T : class
        {
            readonly ParseResult<T> result; 

            public Scenario(params string[] args)
            {
                result = CommandLine.Parse<T>(args);
            }

            public void ShouldSucceed(T expectedModel, params string[] expectedExtraArguments)
            {
                result.Errors.ShouldEqual(new string[] {});
                result.ExtraArguments.ShouldEqual(expectedExtraArguments);
                result.Model.ShouldMatch(expectedModel);
            }

            public void ShouldFail(T expectedModel, params string[] expectedErrors)
            {
                result.Errors.ShouldEqual(expectedErrors);
                result.ExtraArguments.ShouldBeEmpty();
                result.Model.ShouldMatch(expectedModel);
            }

            public void ShouldFail(T expectedModel, string[] expectedExtraArguments, params string[] expectedErrors)
            {
                result.Errors.ShouldEqual(expectedErrors);
                result.ExtraArguments.ShouldBeEmpty();
                result.Model.ShouldMatch(expectedModel);
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
