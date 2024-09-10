using static Fixie.Tests.Assertions.Utility;

namespace Fixie.Tests.Assertions;

class TextAssertionTests
{
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

    public void ShouldAssertStrings()
    {
        "a☺".ShouldBe("a☺");
        Contradiction("a☺", x => x.ShouldBe("z☺"),
            """
            x should be "z☺" but was "a☺"
            """);

        ((string?)null).ShouldBe(null);
        Contradiction("abc", x => x.ShouldBe(null),
            """
            x should be null but was "abc"
            """);
        Contradiction(((string?)null), x => x.ShouldBe("abc"),
            """
            x should be "abc" but was null
            """);
        
        "\u0020 ".ShouldBe("  ");
        Contradiction("abc", x => x.ShouldBe("\u0020 "),
            """
            x should be "  " but was "abc"
            """);

        "\u0000\0 \u0007\a \u0008\b \u0009\t \u000A\n \u000D\r".ShouldBe("\0\0 \a\a \b\b \t\t \n\n \r\r");
        Contradiction("abc", x => x.ShouldBe("\0\a\b\t\n\r"),
            """
            x should be "\0\a\b\t\n\r" but was "abc"
            """);

        // TODO: In C# 13, include \u001B\e becoming \e\e
        "\u000C\f \u000B\v \u0022\" \u0027\' \u005C\\".ShouldBe("\f\f \v\v \"\" \'\' \\\\");
        // TODO: In C# 13, include \e being preserved.
        Contradiction("abc", x => x.ShouldBe("\f\v\"\'\\"),
            """
            x should be "\f\v\"\'\\" but was "abc"
            """);

        foreach (var c in UnicodeEscapedCharacters())
        {
            var s = c.ToString();

            s.ShouldBe(s);
            Contradiction("a", x => x.ShouldBe(s),
                $"""
                 x should be "\u{(int)c:X4}" but was "a"
                 """);
        }
    }

    public void ShouldAssertMultilineStrings()
    {
        var original =
            """
            Line 1
            Line 2
            Line 3
            Line 4
            """;

        var altered =
            """
            Line 1
            Line 2 Altered
            Line 3
            Line 4
            """;

        var mixedLineEndings = "\r \n \r\n \n \r";

        original.ShouldBe(original);
        altered.ShouldBe(altered);

        Contradiction(original, x => x.ShouldBe(altered),
            """"
            x should be
                """
                Line 1
                Line 2 Altered
                Line 3
                Line 4
                """

            but was
                """
                Line 1
                Line 2
                Line 3
                Line 4
                """
            """");

        Contradiction(original, x => x.ShouldBe(mixedLineEndings),
            """"
             x should be
                 "\r \n \r\n \n \r"

             but was
                 """
                 Line 1
                 Line 2
                 Line 3
                 Line 4
                 """
             """");

        Contradiction(mixedLineEndings, x => x.ShouldBe(original),
            """"
             x should be
                 """
                 Line 1
                 Line 2
                 Line 3
                 Line 4
                 """

             but was
                 "\r \n \r\n \n \r"
             """");

        var apparentEscapeSequences =
            """
            \u0020
            \u0000\0 \u0007\a \u0008\b \u0009\t \u000A\n \u000D\r
            \u000C\f \u000B\v \u001B\e \u0022\" \u0027\' \u005C\\
            """;

        Contradiction(apparentEscapeSequences, x => x.ShouldBe(original),
            """"
            x should be
                """
                Line 1
                Line 2
                Line 3
                Line 4
                """

            but was
                """
                \u0020
                \u0000\0 \u0007\a \u0008\b \u0009\t \u000A\n \u000D\r
                \u000C\f \u000B\v \u001B\e \u0022\" \u0027\' \u005C\\
                """
            """");

        var containsApparentOneQuotedRawLiteral =
            """
            "
            Value contains an apparent one-quotes bounded raw string literal.
            "
            """;

        var containsApparentTwoQuotedRawLiteral =
            """
            ""
            Value contains an apparent two-quotes bounded raw string literal.
            ""
            """;

        var containsApparentThreeQuotedRawLiteral =
            """"
            """
            Value contains an apparent three-quotes bounded raw string literal.
            """
            """";

        var containsApparentFourQuotedRawLiteral =
            """""
            """"
            Value contains an apparent four-quotes bounded raw string literal.
            """"
            """"";

         Contradiction(containsApparentTwoQuotedRawLiteral, x => x.ShouldBe(containsApparentOneQuotedRawLiteral),
             """"
             x should be
                 """
                 "
                 Value contains an apparent one-quotes bounded raw string literal.
                 "
                 """

             but was
                 """
                 ""
                 Value contains an apparent two-quotes bounded raw string literal.
                 ""
                 """
             """");

        Contradiction(containsApparentThreeQuotedRawLiteral, x => x.ShouldBe(containsApparentTwoQuotedRawLiteral),
            """""
            x should be
                """
                ""
                Value contains an apparent two-quotes bounded raw string literal.
                ""
                """

            but was
                """"
                """
                Value contains an apparent three-quotes bounded raw string literal.
                """
                """"
            """"");

        Contradiction(containsApparentFourQuotedRawLiteral, x => x.ShouldBe(containsApparentThreeQuotedRawLiteral),
            """"""
            x should be
                """"
                """
                Value contains an apparent three-quotes bounded raw string literal.
                """
                """"

            but was
                """""
                """"
                Value contains an apparent four-quotes bounded raw string literal.
                """"
                """""
            """""");
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
}