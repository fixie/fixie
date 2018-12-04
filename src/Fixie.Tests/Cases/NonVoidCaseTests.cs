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
        public void ShouldIgnoreCaseReturnValuesByDefault()
        {
            using (var console = new RedirectedConsole())
            {
                Run<SampleTestClass>()
                    .ShouldEqual(
                        For<SampleTestClass>(
                            ".BoolFalse passed",
                            ".BoolThrow failed: 'BoolThrow' failed!",
                            ".BoolTrue passed",
                            ".Pass passed",
                            ".String passed",
                            ".StringNull passed",
                            ".Throw failed: 'Throw' failed!"));

                Run<SampleAsyncTestClass>()
                    .ShouldEqual(
                        For<SampleAsyncTestClass>(
                            ".BoolFalse passed",
                            ".BoolThrow failed: 'BoolThrow' failed!",
                            ".BoolTrue passed",
                            ".Pass passed",
                            ".String passed",
                            ".StringNull passed",
                            ".Throw failed: 'Throw' failed!"));

                console.Output.ShouldBeEmpty();
            }
        }

        public void ShouldProvideCaseReturnValuesToCustomBehaviors()
        {
            using (var console = new RedirectedConsole())
            {
                var discovery = new SelfTestDiscovery();
                var execution = new TreatBoolReturnValuesAsAssertions();

                Run<SampleTestClass>(discovery, execution)
                    .ShouldEqual(
                        For<SampleTestClass>(
                            ".BoolFalse failed: Boolean test case returned false!",
                            ".BoolThrow failed: 'BoolThrow' failed!",
                            ".BoolTrue passed",
                            ".Pass passed",
                            ".String passed",
                            ".StringNull passed",
                            ".Throw failed: 'Throw' failed!"));

                console.Lines().ShouldEqual(
                    "BoolFalse False",
                    "BoolThrow null",
                    "BoolTrue True",
                    "Pass null",
                    "String ABC",
                    "StringNull null",
                    "Throw null");
            }
        }

        public void ShouldUnpackResultValuesFromStronglyTypedTaskObjectsForAsyncCases()
        {
            using (var console = new RedirectedConsole())
            {
                var discovery = new SelfTestDiscovery();
                var execution = new TreatBoolReturnValuesAsAssertions();

                Run<SampleAsyncTestClass>(discovery, execution)
                    .ShouldEqual(
                        For<SampleAsyncTestClass>(
                            ".BoolFalse failed: Boolean test case returned false!",
                            ".BoolThrow failed: 'BoolThrow' failed!",
                            ".BoolTrue passed",
                            ".Pass passed",
                            ".String passed",
                            ".StringNull passed",
                            ".Throw failed: 'Throw' failed!"));

                console.Lines().ShouldEqual(
                    "BoolFalse False",
                    "BoolThrow null",
                    "BoolTrue True",
                    "Pass null",
                    "String ABC",
                    "StringNull null",
                    "Throw null");
            }
        }

        class SampleTestClass
        {
            public void Throw() { throw new FailureException(); }

            public void Pass() { }

            public bool BoolThrow() { throw new FailureException(); }

            public bool BoolTrue() => true;

            public bool BoolFalse() => false;

            public string String() => "ABC";

            public string StringNull() => null;
        }

        class SampleAsyncTestClass
        {
            public async Task Throw() { ThrowException(); await Awaitable(true); }
            
            public async Task Pass() => await Awaitable(true);

            public async Task<bool> BoolThrow() { ThrowException(); return await Awaitable(true); }
            
            public async Task<bool> BoolTrue() => await Awaitable(true);

            public async Task<bool> BoolFalse() => await Awaitable(false);

            public async Task<string> String()=> await Awaitable("ABC");

            public async Task<string> StringNull() => await Awaitable<string>(null);

            static Task<T> Awaitable<T>(T value)
                => Task.Run(() => value);

            static void ThrowException([CallerMemberName] string member = null)
                => throw new FailureException(member);
        }

        class TreatBoolReturnValuesAsAssertions : Execution
        {
            public void Execute(TestClass testClass)
            {
                testClass.RunCases(@case =>
                {
                    var instance = testClass.Construct();

                    var returnValue = @case.Execute(instance);

                    Console.WriteLine(@case.Method.Name + " " + (returnValue ?? "null"));

                    if (@case.Exception == null && returnValue is bool success && !success)
                        @case.Fail("Boolean test case returned false!");

                    instance.Dispose();
                });
            }
        }
    }
}