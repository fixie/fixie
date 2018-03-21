namespace Fixie.Tests.Cases
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Execution;
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
                            ".Throw failed: 'Throw' failed!"));

                Run<SampleAsyncTestClass>()
                    .ShouldEqual(
                        For<SampleAsyncTestClass>(
                            ".BoolFalse passed",
                            ".BoolThrow failed: 'BoolThrow' failed!",
                            ".BoolTrue passed",
                            ".Pass passed",
                            ".Throw failed: 'Throw' failed!"));

                console.Output.ShouldBeEmpty();
            }
        }

        public void ShouldProvideCaseReturnValuesToCustomBehaviors()
        {
            var convention = new SelfTestConvention();

            using (var console = new RedirectedConsole())
            {
                convention
                    .Lifecycle<TreatBoolReturnValuesAsAssertions>();

                Run<SampleTestClass>(convention)
                    .ShouldEqual(
                        For<SampleTestClass>(
                            ".BoolFalse failed: Boolean test case returned false!",
                            ".BoolThrow failed: 'BoolThrow' failed!",
                            ".BoolTrue passed",
                            ".Pass passed",
                            ".Throw failed: 'Throw' failed!"));

                console.Lines().ShouldEqual(
                    "BoolFalse False",
                    "BoolThrow null",
                    "BoolTrue True",
                    "Pass null",
                    "Throw null");
            }
        }

        public void ShouldUnpackResultValuesFromStronglyTypedTaskObjectsForAsyncCases()
        {
            var convention = new SelfTestConvention();

            using (var console = new RedirectedConsole())
            {
                convention
                    .Lifecycle<TreatBoolReturnValuesAsAssertions>();

                Run<SampleAsyncTestClass>(convention)
                    .ShouldEqual(
                        For<SampleAsyncTestClass>(
                            ".BoolFalse failed: Boolean test case returned false!",
                            ".BoolThrow failed: 'BoolThrow' failed!",
                            ".BoolTrue passed",
                            ".Pass passed",
                            ".Throw failed: 'Throw' failed!"));

                console.Lines().ShouldEqual(
                    "BoolFalse False",
                    "BoolThrow null",
                    "BoolTrue True",
                    "Pass null",
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
        }

        class SampleAsyncTestClass
        {
            public async Task Throw() { ThrowException(); await Bool(true); }
            
            public async Task Pass() { await Bool(true); }
            
            public async Task<bool> BoolThrow() { ThrowException(); return await Bool(true); }
            
            public async Task<bool> BoolTrue() { return await Bool(true); }

            public async Task<bool> BoolFalse() { return await Bool(false); }

            static Task<bool> Bool(bool value)
            {
                return Task.Run(() => value);
            }

            static void ThrowException([CallerMemberName] string member = null)
            {
                throw new FailureException(member);
            }
        }

        class TreatBoolReturnValuesAsAssertions : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                runCases(@case =>
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