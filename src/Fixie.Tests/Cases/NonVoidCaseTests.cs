namespace Fixie.Tests.Cases
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;
    using static Utility;

    public class NonVoidCaseTests
    {
        public async Task ShouldIgnoreCaseReturnValuesByDefault()
        {
            using var console = new RedirectedConsole();

            (await RunAsync<SampleTestClass>())
                .ShouldBe(
                    For<SampleTestClass>(
                        ".BoolFalse passed",
                        ".BoolThrow failed: 'BoolThrow' failed!",
                        ".BoolTrue passed",
                        ".Pass passed",
                        ".String passed",
                        ".StringNull passed",
                        ".Throw failed: 'Throw' failed!"));

            (await RunAsync<SampleAsyncTestClass>())
                .ShouldBe(
                    For<SampleAsyncTestClass>(
                        ".BoolFalse passed",
                        ".BoolThrow failed: 'BoolThrow' failed!",
                        ".BoolTrue passed",
                        ".Pass passed",
                        ".String passed",
                        ".StringNull passed",
                        ".Throw failed: 'Throw' failed!"));

            console.Output.ShouldBe("");
        }

        public async Task ShouldProvideCaseReturnValuesToCustomBehaviors()
        {
            using var console = new RedirectedConsole();

            (await RunAsync<SampleTestClass, TreatBoolReturnValuesAsAssertions>())
                .ShouldBe(
                    For<SampleTestClass>(
                        ".BoolFalse failed: Boolean test case returned false!",
                        ".BoolThrow failed: 'BoolThrow' failed!",
                        ".BoolTrue passed",
                        ".Pass passed",
                        ".String passed",
                        ".StringNull passed",
                        ".Throw failed: 'Throw' failed!"));

            console.Lines().ShouldBe(
                "BoolFalse False",
                "BoolThrow null",
                "BoolTrue True",
                "Pass null",
                "String ABC",
                "StringNull null",
                "Throw null");
        }

        public async Task ShouldUnpackResultValuesFromStronglyTypedTaskObjectsForAsyncCases()
        {
            using var console = new RedirectedConsole();

            (await RunAsync<SampleAsyncTestClass, TreatBoolReturnValuesAsAssertions>())
                .ShouldBe(
                    For<SampleAsyncTestClass>(
                        ".BoolFalse failed: Boolean test case returned false!",
                        ".BoolThrow failed: 'BoolThrow' failed!",
                        ".BoolTrue passed",
                        ".Pass passed",
                        ".String passed",
                        ".StringNull passed",
                        ".Throw failed: 'Throw' failed!"));

            console.Lines().ShouldBe(
                "BoolFalse False",
                "BoolThrow null",
                "BoolTrue True",
                "Pass null",
                "String ABC",
                "StringNull null",
                "Throw null");
        }

        class SampleTestClass
        {
            public bool BoolFalse() => false;
            public bool BoolThrow() { throw new FailureException(); }
            public bool BoolTrue() => true;
            public void Pass() { }
            public string String() => "ABC";
            public string? StringNull() => null;
            public void Throw() { throw new FailureException(); }
        }

        class SampleAsyncTestClass
        {
            public async Task<bool> BoolFalse() => await Awaitable(false);
            public async Task<bool> BoolThrow() { ThrowException(); return await Awaitable(true); }
            public async Task<bool> BoolTrue() => await Awaitable(true);
            public async Task Pass() => await Awaitable(true);
            public async Task<string> String()=> await Awaitable("ABC");
            public async Task<string?> StringNull() => await Awaitable<string?>(null);
            public async Task Throw() { ThrowException(); await Awaitable(true); }

            static Task<T> Awaitable<T>(T value)
                => Task.Run(() => value);

            static void ThrowException([CallerMemberName] string member = default!)
                => throw new FailureException(member);
        }

        class TreatBoolReturnValuesAsAssertions : Execution
        {
            public async Task ExecuteAsync(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                {
                    await test.RunAsync(@case =>
                    {
                        var result = @case.Result;

                        Console.WriteLine(@case.Method.Name + " " + (result ?? "null"));

                        if (@case.Exception == null && result is bool success && !success)
                            @case.Fail("Boolean test case returned false!");
                    });
                }
            }
        }
    }
}