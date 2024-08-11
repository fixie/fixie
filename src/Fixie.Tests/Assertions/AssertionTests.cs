﻿using System.Runtime.CompilerServices;

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

    public void ShouldAssertChars()
    {
        'a'.ShouldBe('a');
        '☺'.ShouldBe('☺');
        Contradiction('a', x => x.ShouldBe('z'), "x should be 'z' but was 'a'");
        
        // Escape Sequence: Null
        '\u0000'.ShouldBe('\0');
        '\0'.ShouldBe('\0');
        Contradiction('\n', x => x.ShouldBe('\0'), "x should be '\\0' but was '\\n'");

        // Escape Sequence: Alert
        '\u0007'.ShouldBe('\a');
        '\a'.ShouldBe('\a');
        Contradiction('\n', x => x.ShouldBe('\a'), "x should be '\\a' but was '\\n'");

        // Escape Sequence: Backspace
        '\u0008'.ShouldBe('\b');
        '\b'.ShouldBe('\b');
        Contradiction('\n', x => x.ShouldBe('\b'), "x should be '\\b' but was '\\n'");

        // Escape Sequence: Horizontal tab
        '\u0009'.ShouldBe('\t');
        '\t'.ShouldBe('\t');
        Contradiction('\n', x => x.ShouldBe('\t'), "x should be '\\t' but was '\\n'");

        // Escape Sequence: New line
        '\u000A'.ShouldBe('\n');
        '\n'.ShouldBe('\n');
        Contradiction('\r', x => x.ShouldBe('\n'), "x should be '\\n' but was '\\r'");

        // Escape Sequence: Vertical tab
        '\u000B'.ShouldBe('\v');
        '\v'.ShouldBe('\v');
        Contradiction('\n', x => x.ShouldBe('\v'), "x should be '\\v' but was '\\n'");

        // Escape Sequence: Form feed
        '\u000C'.ShouldBe('\f');
        '\f'.ShouldBe('\f');
        Contradiction('\n', x => x.ShouldBe('\f'), "x should be '\\f' but was '\\n'");

        // Escape Sequence: Carriage return
        '\u000D'.ShouldBe('\r');
        '\r'.ShouldBe('\r');
        Contradiction('\n', x => x.ShouldBe('\r'), "x should be '\\r' but was '\\n'");

        // TODO: Applicable in C# 13
        // Escape Sequence: Escape
        // '\u001B'.ShouldBe('\e');
        // '\e'.ShouldBe('\e');
        // Contradiction('\n', x => x.ShouldBe('\e'), "x should be '\\e' but was '\\n'");

        // Literal Space
        ' '.ShouldBe(' ');
        '\u0020'.ShouldBe(' ');
        Contradiction('\n', x => x.ShouldBe(' '), "x should be ' ' but was '\\n'");

        // Escape Sequence: Double quote
        '\u0022'.ShouldBe('\"');
        '\"'.ShouldBe('\"');
        Contradiction('\n', x => x.ShouldBe('\"'), "x should be '\\\"' but was '\\n'");

        // Escape Sequence: Single quote
        '\u0027'.ShouldBe('\'');
        '\''.ShouldBe('\'');
        Contradiction('\n', x => x.ShouldBe('\''), "x should be '\\'' but was '\\n'");

        // Escape Sequence: Backslash
        '\u005C'.ShouldBe('\\');
        '\\'.ShouldBe('\\');
        Contradiction('\n', x => x.ShouldBe('\\'), "x should be '\\\\' but was '\\n'");

        foreach (var c in UnicodeEscapedCharacters())
        {
            c.ShouldBe(c);
            Contradiction('a', x => x.ShouldBe(c), $"x should be '\\u{(int)c:X4}' but was 'a'");
        }
    }

    static IEnumerable<char> UnicodeEscapedCharacters()
    {
        // Code points from \u0000 to \u001F, \u007F, and from \u0080 to \u009F are
        // "control characters". Some of these have single-character escape sequences
        // like '\u000A' being equivalent to '\n'. When we omit code points better
        // served by single-character escape sequences, we are left with those deserving
        // '\uHHHH' hex escape sequences.

        for (char c = '\u0001'; c <= '\u0006'; c++) yield return c;
        for (char c = '\u000E'; c <= '\u001F'; c++) yield return c;
        yield return '\u007F';
        for (char c = '\u0080'; c <= '\u009F'; c++) yield return c;

        // Several code points represent various kinds of whitespace. Some of these have
        // single-character escape sequences like '\u0009' being equivalent to '\t', and
        // the single space character ' ' is naturally represented with no need for any
        // escape sequence. All other whitespace code points deserve '\uHHHH' hex escape
        // sequences.

        foreach (char c in (char[])['\u0085', '\u00A0', '\u1680']) yield return c;
        for (char c = '\u2000'; c <= '\u2009'; c++) yield return c;
        foreach (char c in (char[])['\u200A', '\u2028', '\u2029', '\u202F', '\u205F', '\u3000']) yield return c;
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