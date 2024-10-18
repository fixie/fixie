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
        });

        ShouldHaveNames(output,
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
            "CharParametersTestClass.Char('☺')"
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
        });

        ShouldHaveNames(output,
            "StringParametersTestClass.String(\" ' ' \\\" \")",
            "StringParametersTestClass.String(\" \\\\ \\0 \\a \\b \")",
            "StringParametersTestClass.String(\" \\f \\n \\r \\t \\v \")",
            "StringParametersTestClass.String(\" \\0 \\u0085 \\u2028 \\u2029 ☺ \")",
            "StringParametersTestClass.String(\" \\0 \\0 \\0 \\0 \")",
            "StringParametersTestClass.String(\" \\u0085 \\u0085 \\u0085 \\u2028 \\u2029 ☺ \")",
            "StringParametersTestClass.String(\" \\0 \\u0085 \\u2028 \\u2029 ☺ \")"
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