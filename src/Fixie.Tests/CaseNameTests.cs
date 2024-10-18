namespace Fixie.Tests;

public class CaseNameTests
{
    public async Task ShouldBeNamedAfterTheUnderlyingMethod()
    {
        var output = await RunScript<NoParametersTestClass>(async test =>
        {
            await Run(test);
        });

        ShouldHaveNames(output, "NoParametersTestClass.NoParameters");
    }

    public async Task ShouldIncludeParameterValuesInNameWhenTheUnderlyingMethodHasParameters()
    {
        var output = await RunScript<ParameterizedTestClass>(async test =>
        {
            await Run(test,
                123, true, 'a', "with \"quotes\"", "long \"string\" gets truncated",
                null, this, new ObjectWithNullStringRepresentation());
        });

        ShouldHaveNames(output,
            "ParameterizedTestClass.Parameterized(123, true, 'a', \"with \\\"quotes\\\"\", \"long \\\"string\\\" g...\", null, Fixie.Tests.CaseNameTests, Fixie.Tests.CaseNameTests+ObjectWithNullStringRepresentation)");
    }

    public async Task ShouldIncludeEscapeSequencesInNameWhenTheUnderlyingMethodHasCharParameters()
    {
        // Unicode characters 0085, 2028, and 2029 represent line endings Next Line, Line Separator, and Paragraph Separator, respectively.
        // Just like \r and \n, we escape these in order to present a readable string literal. All other unicode sequences pass through
        // with no additional special treatment.

        // \uxxxx - Unicode escape sequence for character with hex value xxxx.
        // \xn[n][n][n] - Unicode escape sequence for character with hex value nnnn (variable length version of \uxxxx).
        // \Uxxxxxxxx - Unicode escape sequence for character with hex value xxxxxxxx (for generating surrogates).

        var output = await RunScript<CharParametersTestClass>(async test =>
        {
            foreach (var c in new[] {'\"', '"', '\''})
                await Run(test, c);
            
            foreach (var c in new[] {'\\', '\0', '\a', '\b', '\f', '\n', '\r', '\t', '\v'})
                await Run(test, c);
            
            foreach (var c in new[] {'\u0000', '\u0085', '\u2028', '\u2029', '\u263A'})
                await Run(test, c);
            
            foreach (var c in new[] {'\x0000', '\x000', '\x00', '\x0'})
                await Run(test, c);
            
            foreach (var c in new[] {'\x0085', '\x085', '\x85', '\x2028', '\x2029', '\x263A'})
                await Run(test, c);
            
            foreach (var c in new[] {'\U00000000', '\U00000085', '\U00002028', '\U00002029', '\U0000263A'})
                await Run(test, c);

            foreach (var c in UnicodeEscapedCharacters())
                await Run(test, c);
        });

        var unicodeEscapeExpectations = UnicodeEscapedCharacters()
            .Select(c =>
            {
                if (c is not ('\u0085' or '\u2028' or '\u2029'))
                {
                    // Characterization coverage of undesirable behavior. Note many control
                    // characters and whitespace characters fail to be escaped.
                    return $"""
                            CharParametersTestClass.Char('{c}')
                            """;
                }

                return $"""
                        CharParametersTestClass.Char('\u{(int)c:X4}')
                        """;
            });

        ShouldHaveNames(output, [
            "CharParametersTestClass.Char('\"')",
            "CharParametersTestClass.Char('\"')",
            "CharParametersTestClass.Char('\\'')",

            "CharParametersTestClass.Char('\\\\')",
            "CharParametersTestClass.Char('\\0')",
            "CharParametersTestClass.Char('\\a')",
            "CharParametersTestClass.Char('\\b')",
            "CharParametersTestClass.Char('\\f')",
            "CharParametersTestClass.Char('\\n')",
            "CharParametersTestClass.Char('\\r')",
            "CharParametersTestClass.Char('\\t')",
            "CharParametersTestClass.Char('\\v')",
            
            "CharParametersTestClass.Char('\\0')",
            "CharParametersTestClass.Char('\\u0085')",
            "CharParametersTestClass.Char('\\u2028')",
            "CharParametersTestClass.Char('\\u2029')",
            "CharParametersTestClass.Char('☺')",

            "CharParametersTestClass.Char('\\0')",
            "CharParametersTestClass.Char('\\0')",
            "CharParametersTestClass.Char('\\0')",
            "CharParametersTestClass.Char('\\0')",

            "CharParametersTestClass.Char('\\u0085')",
            "CharParametersTestClass.Char('\\u0085')",
            "CharParametersTestClass.Char('\\u0085')",
            "CharParametersTestClass.Char('\\u2028')",
            "CharParametersTestClass.Char('\\u2029')",
            "CharParametersTestClass.Char('☺')",

            "CharParametersTestClass.Char('\\0')",
            "CharParametersTestClass.Char('\\u0085')",
            "CharParametersTestClass.Char('\\u2028')",
            "CharParametersTestClass.Char('\\u2029')",
            "CharParametersTestClass.Char('☺')",

            ..unicodeEscapeExpectations
            ]
        );
    }

    public async Task ShouldIncludeEscapeSequencesInNameWhenTheUnderlyingMethodHasStringParameters()
    {
        // Unicode characters 0085, 2028, and 2029 represent line endings Next Line, Line Separator, and Paragraph Separator, respectively.
        // Just like \r and \n, we escape these in order to present a readable string literal. All other unicode sequences pass through
        // with no additional special treatment.

        // \uxxxx - Unicode escape sequence for character with hex value xxxx.
        // \xn[n][n][n] - Unicode escape sequence for character with hex value nnnn (variable length version of \uxxxx).
        // \Uxxxxxxxx - Unicode escape sequence for character with hex value xxxxxxxx (for generating surrogates).

        var output = await RunScript<StringParametersTestClass>(async test =>
        {
            await Run(test, " \' ' \" ");
            await Run(test, " \\ \0 \a \b ");
            await Run(test, " \f \n \r \t \v ");
            await Run(test, " \u0000 \u0085 \u2028 \u2029 \u263A ");
            await Run(test, " \x0000 \x000 \x00 \x0 ");
            await Run(test, " \x0085 \x085 \x85 \x2028 \x2029 \x263A ");
            await Run(test, " \U00000000 \U00000085 \U00002028 \U00002029 \U0000263A ");
            
            foreach (var c in UnicodeEscapedCharacters().Chunk(5).Select(chunk => string.Join(' ', chunk)))
                await Run(test, c);
        });

        ShouldHaveNames(output,
            "StringParametersTestClass.String(\" ' ' \\\" \")",
            "StringParametersTestClass.String(\" \\\\ \\0 \\a \\b \")",
            "StringParametersTestClass.String(\" \\f \\n \\r \\t \\v \")",
            "StringParametersTestClass.String(\" \\0 \\u0085 \\u2028 \\u2029 ☺ \")",
            "StringParametersTestClass.String(\" \\0 \\0 \\0 \\0 \")",
            "StringParametersTestClass.String(\" \\u0085 \\u0085 \\u0085 \\u2028 \\u2029 ☺ \")",
            "StringParametersTestClass.String(\" \\0 \\u0085 \\u2028 \\u2029 ☺ \")",

            // Characterization coverage of undesirable behavior. Note many control
            // characters and whitespace characters fail to be escaped.
            "StringParametersTestClass.String(\"\u0001 \u0002 \u0003 \u0004 \u0005\")",
            "StringParametersTestClass.String(\"\u0006 \u000E \u000F \u0010 \u0011\")",
            "StringParametersTestClass.String(\"\u0012 \u0013 \u0014 \u0015 \u0016\")",
            "StringParametersTestClass.String(\"\u0017 \u0018 \u0019 \u001A \u001B\")",
            "StringParametersTestClass.String(\"\u001C \u001D \u001E \u001F \u007F\")",
            "StringParametersTestClass.String(\"\u0080 \u0081 \u0082 \u0083 \u0084\")",
            "StringParametersTestClass.String(\"\\u0085 \u0086 \u0087 \u0088 \u0089\")",
            "StringParametersTestClass.String(\"\u008A \u008B \u008C \u008D \u008E\")",
            "StringParametersTestClass.String(\"\u008F \u0090 \u0091 \u0092 \u0093\")",
            "StringParametersTestClass.String(\"\u0094 \u0095 \u0096 \u0097 \u0098\")",
            "StringParametersTestClass.String(\"\u0099 \u009A \u009B \u009C \u009D\")",
            "StringParametersTestClass.String(\"\u009E \u009F \\u0085 \u00A0 \u1680\")",
            "StringParametersTestClass.String(\"\u2000 \u2001 \u2002 \u2003 \u2004\")",
            "StringParametersTestClass.String(\"\u2005 \u2006 \u2007 \u2008 \u2009\")",
            "StringParametersTestClass.String(\"\u200A \\u2028 \\u2029 \u202F \u205F\")",
            "StringParametersTestClass.String(\"\u3000\")"
        );
    }

    public async Task ShouldIncludeResolvedGenericArgumentsInNameWhenTheUnderlyingMethodIsGeneric()
    {
        var output = await RunScript<GenericTestClass>(async test =>
        {
            await Run(test, 123, true, "a", "b");
            await Run(test, 123, true, 1, null);
            await Run(test, 123, 1.23m, "a", null);
        });

        ShouldHaveNames(output,
            "GenericTestClass.Generic<System.Boolean, System.String>(123, true, \"a\", \"b\")",
            "GenericTestClass.Generic<System.Boolean, System.Int32>(123, true, 1, null)",
            "GenericTestClass.Generic<System.Decimal, System.String>(123, 1.23, \"a\", null)"
        );
    }

    public async Task ShouldUseGenericTypeParametersInNameWhenGenericTypeParametersCannotBeResolved()
    {
        var output = await RunScript<ConstrainedGenericTestClass>(async test =>
        {
            await Run(test, 1);
            await Run(test, true);
            await Run(test, "Incompatible");
        });

        ShouldHaveNames(output,
            "ConstrainedGenericTestClass.ConstrainedGeneric<System.Int32>(1)",
            "ConstrainedGenericTestClass.ConstrainedGeneric<System.Boolean>(true)",
            "ConstrainedGenericTestClass.ConstrainedGeneric<T>(\"Incompatible\")"
        );
    }

    public async Task ShouldInferAppropriateClassUnderInheritance()
    {
        var parent = await RunScript<ParentTestClass>(async test =>
        {
            await Run(test);
        });

        ShouldHaveNames(parent,
            "ParentTestClass.TestMethodDefinedWithinParentClass"
        );

        var child = await RunScript<ChildTestClass>(async test =>
        {
            await Run(test);
        });

        ShouldHaveNames(child,
            "ChildTestClass.TestMethodDefinedWithinChildClass",
            "ChildTestClass.TestMethodDefinedWithinParentClass"
        );
    }

    class ScriptedExecution : IExecution
    {
        readonly Func<Test, Task> script;

        public ScriptedExecution(Func<Test, Task> script)
            => this.script = script;

        public async Task Run(TestSuite testSuite)
        {
            foreach (var test in testSuite.Tests)
                await script(test);
        }
    }

    static async Task<IEnumerable<string>> RunScript<TSampleTestClass>(Func<Test, Task> scriptAsync)
    {
        await using var console = new StringWriter();
        return await Utility.Run(typeof(TSampleTestClass), new ScriptedExecution(scriptAsync), console);
    }

    static async Task Run(Test test, params object?[] parameters)
    {
        await test.Run(parameters);
        await test.Pass(parameters);
        await test.Fail(parameters, new FailureException());
        await test.Skip(parameters, reason: "Exercising Skipped Case Names");
    }

    void ShouldHaveNames(IEnumerable<string> actual, params string[] expected)
    {
        var expectedVariants = expected.Select(name => new[]
        {
            name.Contains("Incompatible")
                ? $"{name} failed: The type parameters for generic method ConstrainedGeneric<T>(T) could not be resolved."
                : $"{name} passed",
            $"{name} passed",
            $"{name} failed: 'Run' failed!",
            $"{name} skipped: Exercising Skipped Case Names"
        }).SelectMany(x => x);

        var fullyQualifiedExpectation = expectedVariants.Select(x => GetType().FullName + "+" + x).ToArray();

        actual.ToArray().ShouldMatch(fullyQualifiedExpectation);
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

    class ObjectWithNullStringRepresentation
    {
        public override string? ToString() => null;
    }

    class NoParametersTestClass
    {
        public void NoParameters() { }
    }

    class ParameterizedTestClass
    {
        public void Parameterized(int i, bool b, char ch, string s1, string s2, object obj, CaseNameTests complex, ObjectWithNullStringRepresentation nullStringRepresentation) { }
    }

    class CharParametersTestClass
    {
        public void Char(char c) { }
    }

    class StringParametersTestClass
    {
        public void String(string s) { }
    }

    class GenericTestClass
    {
        public void Generic<T1, T2>(int i, T1 t1, T2 t2a, T2 t2b) { }
    }

    class ConstrainedGenericTestClass
    {
        public void ConstrainedGeneric<T>(T t) where T : struct { }
    }

    class ParentTestClass
    {
        public void TestMethodDefinedWithinParentClass()
        {
        }
    }

    class ChildTestClass : ParentTestClass
    {
        public void TestMethodDefinedWithinChildClass()
        {
        }
    }
}