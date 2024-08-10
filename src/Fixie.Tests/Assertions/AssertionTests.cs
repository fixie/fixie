using System.Runtime.CompilerServices;

namespace Fixie.Tests.Assertions;

public class AssertionTests
{
    static readonly string Line = Environment.NewLine + Environment.NewLine;

    public void ShouldAssertBools()
    {
        true.ShouldBe(true);
        false.ShouldBe(false);

        Contradiction(true, x => x.ShouldBe(false), "x should be false but was true");
        Contradiction(false, x => x.ShouldBe(true), "x should be true but was false");
    }

    static void Contradiction<T>(T actual, Action<T> shouldThrow, string expectedMessage, [CallerArgumentExpression(nameof(shouldThrow))] string? assertion = null)
    {
        try
        {
            shouldThrow(actual);
        }
        catch (Exception exception)
        {
            if (exception is AssertException)
            {
                if (exception.Message != expectedMessage)
                    throw new Exception(
                        $"An example assertion failed as expected, but with the wrong message.{Line}" +
                        $"Expected Message:{Line}\t{expectedMessage}{Line}" +
                        $"Actual Message:{Line}\t{exception.Message}");
                return;
            }

            throw new Exception(
                $"An example assertion failed as expected, but with the wrong type.{Line}" +
                $"\t{assertion}{Line}" +
                $"The actual value in question was:{Line}" +
                $"\t{actual}{Line}" +
                $"The assertion threw {exception.GetType().FullName} with message:{Line}" +
                $"\t{exception.Message}");
        }

        throw new Exception(
            $"An example assertion was expected to fail, but did not:{Line}" +
            $"\t{assertion}{Line}" +
            $"The actual value in question was:{Line}" +
            $"\t{actual}");
    }
}