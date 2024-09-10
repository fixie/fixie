using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.Environment;

namespace Fixie.Tests.Assertions;

static class Utility
{
    static readonly string Line = NewLine + NewLine;

    public static void Contradiction<T>(T actual, Action<T> shouldThrow, string expectedMessage, [CallerArgumentExpression(nameof(shouldThrow))] string? assertion = null)
    {
        try
        {
            shouldThrow(actual);
        }
        catch (Exception exception)
        {
            ShouldBeAssertException(actual, expectedMessage, assertion, exception);
            return;
        }

        ShouldHaveFailedAssertion(actual, assertion);

        throw new UnreachableException();
    }

    public static async Task Contradiction<T>(T actual, Func<T, Task> shouldThrowAsync, string expectedMessage, [CallerArgumentExpression(nameof(shouldThrowAsync))] string? assertion = null)
    {
        try
        {
            await shouldThrowAsync(actual);
        }
        catch (Exception exception)
        {
            ShouldBeAssertException(actual, expectedMessage, assertion, exception);
            return;
        }

        ShouldHaveFailedAssertion(actual, assertion);

        throw new UnreachableException();
    }

    static void ShouldBeAssertException<T>(T actual, string expectedMessage, string? assertion, Exception exception)
    {
        if (exception is AssertException)
        {
            if (exception.Message != expectedMessage)
                throw new Exception(
                    $"An example assertion failed as expected, but with the wrong message.{Line}" +
                    $"Expected Message:{Line}{Indent(expectedMessage)}{Line}" +
                    $"Actual Message:{Line}{Indent(exception.Message)}");

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

    static void ShouldHaveFailedAssertion<T>(T actual, string? assertion)
    {
        throw new Exception(
            $"An example assertion was expected to fail, but did not:{Line}" +
            $"\t{assertion}{Line}" +
            $"The actual value in question was:{Line}" +
            $"\t{actual}");
    }

    static string Indent(string multiline) =>
        string.Join(NewLine, multiline.Split(NewLine).Select(x => $"\t{x}"));
}