using static Fixie.Tests.Assertions.Utility;
using static Fixie.Tests.Utility;

namespace Fixie.Tests.Assertions;

public class AssertionTests
{
    public void ShouldAssertEquatables()
    {
        HttpMethod.Post.ShouldBe(HttpMethod.Post);
        Contradiction(HttpMethod.Post, x => x.ShouldBe(HttpMethod.Get), "x should be GET but was POST");
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

    public void ShouldAssertTypes()
    {
        typeof(int).ShouldBe(typeof(int));
        typeof(char).ShouldBe(typeof(char));
        Contradiction(typeof(Utility), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(Fixie.Tests.Assertions.Utility)");
        Contradiction(typeof(bool), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(bool)");
        Contradiction(typeof(sbyte), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(sbyte)");
        Contradiction(typeof(byte), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(byte)");
        Contradiction(typeof(short), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(short)");
        Contradiction(typeof(ushort), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(ushort)");
        Contradiction(typeof(int), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(int)");
        Contradiction(typeof(uint), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(uint)");
        Contradiction(typeof(long), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(long)");
        Contradiction(typeof(ulong), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(ulong)");
        Contradiction(typeof(nint), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(nint)");
        Contradiction(typeof(nuint), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(nuint)");
        Contradiction(typeof(decimal), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(decimal)");
        Contradiction(typeof(double), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(double)");
        Contradiction(typeof(float), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(float)");
        Contradiction(typeof(char), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(char)");
        Contradiction(typeof(string), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(string)");
        Contradiction(typeof(object), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(object)");

        1.ShouldBe<int>();
        'A'.ShouldBe<char>();
        Exception exception = new DivideByZeroException();
        DivideByZeroException typedException = exception.ShouldBe<DivideByZeroException>();
        Contradiction(new StubReport(), x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(Fixie.Tests.StubReport)");
        Contradiction(true, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(bool)");
        Contradiction((sbyte)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(sbyte)");
        Contradiction((byte)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(byte)");
        Contradiction((short)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(short)");
        Contradiction((ushort)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(ushort)");
        Contradiction((int)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(int)");
        Contradiction((uint)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(uint)");
        Contradiction((long)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(long)");
        Contradiction((ulong)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(ulong)");
        Contradiction((nint)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(nint)");
        Contradiction((nuint)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(nuint)");
        Contradiction((decimal)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(decimal)");
        Contradiction((double)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(double)");
        Contradiction((float)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(float)");
        Contradiction((char)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(char)");
        Contradiction("A", x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(string)");
        Contradiction(new object(), x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(object)");
        Contradiction((Exception?)null, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was null");
        Contradiction((AssertionTests?)null, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was null");
    }

    public void ShouldAssertObjects()
    {
        var objectA = new SampleA();
        var objectB = new SampleB();

        objectA.ShouldBe(objectA);
        objectB.ShouldBe(objectB);

        Contradiction(objectB, x => x.ShouldBe((object?)null),
            $"x should be null but was {FullName<SampleB>()}");
        Contradiction(objectB, x => x.ShouldBe(objectA),
            $"x should be {FullName<SampleA>()} but was {FullName<SampleB>()}");
        Contradiction(objectA, x => x.ShouldBe(objectB),
            $"x should be {FullName<SampleB>()} but was {FullName<SampleA>()}");
    }

    public void ShouldAssertNulls()
    {
        object? nullObject = null;
        object nonNullObject = new SampleA();

        nullObject.ShouldBe(null);
        nonNullObject.ShouldNotBeNull();

        Contradiction((object?)null, x => x.ShouldBe(nonNullObject),
            $"x should be {FullName<SampleA>()} but was null");
        Contradiction(nonNullObject, x => x.ShouldBe(null),
            $"x should be null but was {FullName<SampleA>()}");
        Contradiction((object?)null, x => x.ShouldNotBeNull(),
            "x should not be null but was null");
    }

    public void ShouldAssertLists()
    {
        new int[]{}.ShouldBe([]);

        Contradiction(new[] { 0 }, x => x.ShouldBe([]),
            """
            x should be
                [
                
                ]

            but was
                [
                    0
                ]
            """);

        Contradiction(new int[] { }, x => x.ShouldBe([0]),
            """
            x should be
                [
                    0
                ]

            but was
                [
                
                ]
            """);

        new[] { false, true, false }.ShouldBe([false, true, false]);

        Contradiction(new[] { false, true, false }, x => x.ShouldBe([false, true]),
            """
            x should be
                [
                    false,
                    true
                ]

            but was
                [
                    false,
                    true,
                    false
                ]
            """);
        
        new[] { 'A', 'B', 'C' }.ShouldBe(['A', 'B', 'C']);
        
        Contradiction(new[] { 'A', 'B', 'C' }, x => x.ShouldBe(['A', 'C']),
            """
            x should be
                [
                    'A',
                    'C'
                ]

            but was
                [
                    'A',
                    'B',
                    'C'
                ]
            """);

        new[] { "A", "B", "C" }.ShouldBe(["A", "B", "C"]);

        Contradiction(new[] { "A", "B", "C" }, x => x.ShouldBe(["A", "C"]),
            """
            x should be
                [
                    "A",
                    "C"
                ]

            but was
                [
                    "A",
                    "B",
                    "C"
                ]
            """);

        new[] { typeof(int), typeof(bool) }.ShouldBe([typeof(int), typeof(bool)]);

        Contradiction(new[] { typeof(int), typeof(bool) }, x => x.ShouldBe([typeof(bool), typeof(int)]),
            """
            x should be
                [
                    typeof(bool),
                    typeof(int)
                ]
            
            but was
                [
                    typeof(int),
                    typeof(bool)
                ]
            """);

        var sampleA = new Sample("A");
        var sampleB = new Sample("B");

        new[] { sampleA, sampleB }.ShouldBe([sampleA, sampleB]);

        Contradiction(new[] { sampleA, sampleB }, x => x.ShouldBe([sampleB, sampleA]),
            """
            x should be
                [
                    Sample B,
                    Sample A
                ]

            but was
                [
                    Sample A,
                    Sample B
                ]
            """);
    }

    public void ShouldAssertExpressions()
    {
        Contradiction(3, value => value.Should(x => x > 4), "value should be > 4 but was 3");
        Contradiction(4, value => value.Should(y => y > 4), "value should be > 4 but was 4");
        5.Should(x => x > 4);

        Contradiction(3, value => value.Should(abc => abc >= 4), "value should be >= 4 but was 3");
        4.Should(x => x >= 4);
        5.Should(x => x >= 4);

        Func<int, bool> someExpression = x => x >= 4;
        Contradiction(3, value => value.Should(someExpression), "value should be someExpression but was 3");

        var a1 = new SampleA();
        var a2 = new SampleA();

        a1.Should(x => x == a1);
        Contradiction(a1, value => value.Should(x => x == a2), "value should be == a2 but was Fixie.Tests.Assertions.AssertionTests+SampleA");
    }

    public void ShouldAssertExpectedExceptions()
    {
        var doNothing = () => { };
        Action divideByZero = () => throw new DivideByZeroException("Divided By Zero");

        divideByZero.ShouldThrow<DivideByZeroException>("Divided By Zero")
            .ShouldBe<DivideByZeroException>();

        divideByZero.ShouldThrow<Exception>("Divided By Zero")
            .ShouldBe<DivideByZeroException>();

        Contradiction(doNothing, noop => noop.ShouldThrow<DivideByZeroException>("Divided By Zero"),
            """
            noop should have thrown System.DivideByZeroException but did not
            """);

        Contradiction(divideByZero, divide => divide.ShouldThrow<DivideByZeroException>("Divided By One Minus One"),
            """
            divide should have thrown System.DivideByZeroException with message
            
                "Divided By One Minus One"
            
            but instead the message was
            
                "Divided By Zero"
            """);

        Contradiction(divideByZero, divide => divide.ShouldThrow<ArgumentNullException>("Argument Null"),
            """
            divide should have thrown System.ArgumentNullException with message
            
                "Argument Null"
            
            but instead it threw System.DivideByZeroException with message
            
                "Divided By Zero"
            """);
    }

    public async Task ShouldAssertExpectedAsyncExceptions()
    {
        var doNothing = async () => { await Task.CompletedTask; };
        var divideByZero = async () =>
        {
            await Task.CompletedTask;
            throw new DivideByZeroException("Divided By Zero");
        };

        var actualDivideByZeroException = await divideByZero.ShouldThrow<DivideByZeroException>("Divided By Zero");
        actualDivideByZeroException.ShouldBe<DivideByZeroException>();

        var actualException = await divideByZero.ShouldThrow<Exception>("Divided By Zero");
        actualException.ShouldBe<DivideByZeroException>();

        await Contradiction(doNothing, noop => noop.ShouldThrow<DivideByZeroException>("Divided By Zero"),
            """
            noop should have thrown System.DivideByZeroException but did not
            """);

        await Contradiction(divideByZero, divide => divide.ShouldThrow<DivideByZeroException>("Divided By One Minus One"),
            """
            divide should have thrown System.DivideByZeroException with message
            
                "Divided By One Minus One"

            but instead the message was
            
                "Divided By Zero"
            """);

        await Contradiction(divideByZero, divide => divide.ShouldThrow<ArgumentNullException>("Argument Null"),
            """
            divide should have thrown System.ArgumentNullException with message
            
                "Argument Null"

            but instead it threw System.DivideByZeroException with message
            
                "Divided By Zero"
            """);
    }

    class SampleA;
    class SampleB;

    class Sample(string name)
    {
        public override string ToString() => $"Sample {name}";
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