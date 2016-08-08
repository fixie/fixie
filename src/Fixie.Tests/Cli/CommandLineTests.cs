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

        public void ShouldFailWithClearExplanationWhenArgumentsAreNotConvertibleToParameterType()
        {
            Parse<ModelWithArguments<int>>("1", "2", "abc")
                .ShouldFail(new ModelWithArguments<int>(1, 2, 0),
                    "Expected third to be convertible to System.Int32.");
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
                result.Model.ShouldMatch(expectedModel);
                result.ExtraArguments.ShouldEqual(expectedExtraArguments);
                result.Errors.ShouldBeEmpty();
            }

            public void ShouldFail(T expectedModel, params string[] expectedErrors)
            {
                result.Model.ShouldMatch(expectedModel);
                result.ExtraArguments.ShouldBeEmpty();
                result.Errors.ShouldEqual(expectedErrors);
            }

            public void ShouldFail(T expectedModel, string[] expectedExtraArguments, params string[] expectedErrors)
            {
                result.Model.ShouldMatch(expectedModel);
                result.ExtraArguments.ShouldBeEmpty();
                result.Errors.ShouldEqual(expectedErrors);
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
