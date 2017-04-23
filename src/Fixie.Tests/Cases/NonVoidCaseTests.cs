namespace Fixie.Tests.Cases
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Execution;

    public class NonVoidCaseTests : CaseTests
    {
        public void ShouldIgnoreCaseReturnValuesByDefault()
        {
            using (var console = new RedirectedConsole())
            {
                Run<SampleTestClass>();
                Run<SampleAsyncTestClass>();

                var expectedSyncEntries = For<SampleTestClass>(
                    ".BoolFalse passed",
                    ".BoolThrow failed: 'BoolThrow' failed!",
                    ".BoolTrue passed",
                    ".Pass passed",
                    ".Throw failed: 'Throw' failed!");

                var expectedAsyncEntries = For<SampleAsyncTestClass>(
                    ".BoolFalse passed",
                    ".BoolThrow failed: 'BoolThrow' failed!",
                    ".BoolTrue passed",
                    ".Pass passed",
                    ".Throw failed: 'Throw' failed!");

                Listener.Entries.ShouldEqual(
                    expectedSyncEntries.Concat(
                        expectedAsyncEntries).ToArray());

                console.Output.ShouldBeEmpty();
            }
        }

        public void ShouldProvideCaseReturnValuesToCustomBehaviors()
        {
            using (var console = new RedirectedConsole())
            {
                Convention
                    .CaseExecution
                    .Wrap<TreatBoolReturnValuesAsAssertions>();

                Run<SampleTestClass>();

                Listener.Entries.ShouldEqual(
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
            using (var console = new RedirectedConsole())
            {
                Convention
                    .CaseExecution
                    .Wrap<TreatBoolReturnValuesAsAssertions>();

                Run<SampleAsyncTestClass>();

                Listener.Entries.ShouldEqual(
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

        class TreatBoolReturnValuesAsAssertions : CaseBehavior
        {
            public void Execute(Case @case, Action next)
            {
                next();

                Console.WriteLine(@case.Method.Name + " " + (@case.ReturnValue ?? "null"));

                if (@case.Exceptions.Any())
                    return;

                if (@case.ReturnValue is bool)
                    if (!(bool)@case.ReturnValue)
                        throw new Exception("Boolean test case returned false!");
            }
        }
    }
}