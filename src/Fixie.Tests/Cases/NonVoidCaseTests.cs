using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Should;

namespace Fixie.Tests.Cases
{
    public class NonVoidCaseTests : CaseTests
    {
        public void ShouldIgnoreCaseReturnValuesByDefault()
        {
            using (var console = new RedirectedConsole())
            {
                Run<SampleTestClass>();
                Run<SampleAsyncTestClass>();

                Listener.Entries.ShouldEqual(
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleTestClass.BoolFalse passed.",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleTestClass.BoolThrow failed: 'BoolThrow' failed!",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleTestClass.BoolTrue passed.",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleTestClass.Pass passed.",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleTestClass.Throw failed: 'Throw' failed!",

                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleAsyncTestClass.BoolFalse passed.",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleAsyncTestClass.BoolThrow failed: 'BoolThrow' failed!",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleAsyncTestClass.BoolTrue passed.",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleAsyncTestClass.Pass passed.",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleAsyncTestClass.Throw failed: 'Throw' failed!"
                    );

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
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleTestClass.BoolFalse failed: Boolean test case returned false!",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleTestClass.BoolThrow failed: 'BoolThrow' failed!",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleTestClass.BoolTrue passed.",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleTestClass.Pass passed.",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleTestClass.Throw failed: 'Throw' failed!"
                    );

                console.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ShouldEqual(
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
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleAsyncTestClass.BoolFalse failed: Boolean test case returned false!",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleAsyncTestClass.BoolThrow failed: 'BoolThrow' failed!",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleAsyncTestClass.BoolTrue passed.",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleAsyncTestClass.Pass passed.",
                    "Fixie.Tests.Cases.NonVoidCaseTests+SampleAsyncTestClass.Throw failed: 'Throw' failed!"
                    );

                console.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ShouldEqual(
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

            public bool BoolTrue() { return true; }

            public bool BoolFalse() { return false; }
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

                Console.WriteLine(@case.Method.Name + " " + (@case.Result ?? "null"));

                if (@case.Execution.Exceptions.Any())
                    return;

                if (@case.Result is bool)
                    if (!(bool)@case.Result)
                        throw new Exception("Boolean test case returned false!");
            }
        }
    }
}