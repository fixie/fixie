namespace Fixie.Tests.Cli
{
    using System;
    using Assertions;
    using Fixie.Cli;

    public class CommandLineTests
    {
        class Empty { }

        public void ShouldParseEmptyModels()
        {
            Parse<Empty>().ShouldSucceed(new Empty());

            Parse<Empty>("first", "second", "third", "fourth", "fifth")
                .ShouldSucceed(new Empty(), "first", "second", "third", "fourth", "fifth");
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
                unusedArguments.ShouldEqual(expectedUnusedArguments);
            }

            public void ShouldFail(string expectedExceptionMessage)
            {
                Action shouldThrow = () => CommandLine.Parse<T>(arguments);

                shouldThrow.ShouldThrow<CommandLineException>(expectedExceptionMessage);
            }

            static void ShouldMatch(T actual, T expected)
            {
                expected.ShouldNotBeNull();
                actual.ShouldNotBeNull();

                foreach (var property in typeof(T).GetProperties())
                {
                    var actualValue = property.GetValue(actual);
                    var expectedValue = property.GetValue(expected);

                    actualValue.ShouldEqual(expectedValue);
                }
            }
        }
    }
}