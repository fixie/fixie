namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Execution;

    public class LifecycleTests
    {
        static string[] FailingMembers;
        readonly Convention Convention;

        public LifecycleTests()
        {
            FailingMembers = null;

            Convention = new Convention();
            Convention.Classes.Where(testClass => testClass == typeof(SampleTestClass) || testClass == typeof(ParameterizedSampleTestClass));
            Convention.Methods.OrderByDescending(x => x.Name, StringComparer.Ordinal);
        }

        static void FailDuring(params string[] failingMemberNames)
        {
            FailingMembers = failingMemberNames;
        }

        Output Run<TSampleTestClass>()
        {
            var listener = new StubListener();

            using (var console = new RedirectedConsole())
            {
                Utility.Run<TSampleTestClass>(listener, Convention);

                return new Output(console.Lines().ToArray(), listener.Entries.ToArray());
            }
        }

        class Output
        {
            readonly string[] lifecycle;
            readonly string[] results;

            public Output(string[] lifecycle, string[] results)
            {
                this.lifecycle = lifecycle;
                this.results = results;
            }

            public void ShouldHaveLifecycle(params string[] expected)
            {
                lifecycle.ShouldEqual(expected);
            }

            public void ShouldHaveResults(params string[] expected)
            {
                var namespaceQualifiedExpectation = expected.Select(x => "Fixie.Tests.LifecycleTests+" + x).ToArray();

                results.ShouldEqual(namespaceQualifiedExpectation);
            }
        }

        class SampleTestClass : IDisposable
        {
            bool disposed;

            public SampleTestClass()
            {
                WhereAmI();
            }

            public void Pass()
            {
                WhereAmI();
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void Dispose()
            {
                if (disposed)
                    throw new ShouldBeUnreachableException();
                disposed = true;

                WhereAmI();
            }
        }

        class ParameterizedSampleTestClass : IDisposable
        {
            bool disposed;

            public ParameterizedSampleTestClass()
            {
                WhereAmI();
            }

            public void IntArg(int i)
            {
                WhereAmI();
                if (i != 0)
                    throw new Exception("Expected 0, but was " + i);
            }

            public void BoolArg(bool b)
            {
                WhereAmI();
                if (!b)
                    throw new Exception("Expected true, but was false");
            }

            public void Dispose()
            {
                if (disposed)
                    throw new ShouldBeUnreachableException();
                disposed = true;

                WhereAmI();
            }
        }

        static void WhereAmI([CallerMemberName] string member = null)
        {
            Console.WriteLine(member);

            if (FailingMembers != null && FailingMembers.Contains(member))
                throw new FailureException(member);
        }

        class CreateInstancePerCase : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                runCases(@case =>
                {
                    var instance = testClass.Construct();

                    @case.Execute(instance);

                    (instance as IDisposable)?.Dispose();
                });
            }
        }

        class CreateInstancePerClass : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                var instance = testClass.Construct();

                runCases(@case =>
                {
                    CaseSetUp(@case);
                    @case.Execute(instance);
                    CaseTearDown(@case);
                });

                (instance as IDisposable)?.Dispose();
            }

            static void CaseSetUp(Case @case)
            {
                @case.Class.ShouldEqual(typeof(SampleTestClass));
                WhereAmI();
            }

            static void CaseTearDown(Case @case)
            {
                @case.Class.ShouldEqual(typeof(SampleTestClass));
                WhereAmI();
            }
        }

        class BuggyLifecycle : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
                => throw new Exception("Unsafe lifecycle threw!");
        }

        class ShortCircuitClassExecution : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                //Class lifecycle chooses not to invoke runCases(...).
                //Since the test cases never run, they are all considered
                //'skipped'.
            }
        }

        class ShortCircuitCaseExection : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                runCases(@case =>
                {
                    //Case lifecycle chooses not to invoke @case.Execute(instance).
                    //Since the test cases never run, they are all considered
                    //'skipped'.
                });
            }
        }

        class RunCasesTwice : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                var instance = testClass.Construct();

                runCases(@case => @case.Execute(instance));
                runCases(@case => @case.Execute(instance));
            }
        }

        class RetryFailingCases : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                var instance = testClass.Construct();

                runCases(@case =>
                {
                    @case.Execute(instance);

                    if (@case.Exception != null)
                        @case.Execute(instance);
                });
            }
        }

        class BuggyParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                throw new Exception("Exception thrown while attempting to yield input parameters for method: " + method.Name);
            }
        }

        public void ShouldConstructPerCaseByDefault()
        {
            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerCaseUsingLifecycleType()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerClassUsingLifecycleType()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldAllowConstructingPerCaseUsingLifecycleInstance()
        {
            Convention.ClassExecution.Lifecycle(new CreateInstancePerCase());

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerClassUsingLifecycleInstance()
        {
            Convention.ClassExecution.Lifecycle(new CreateInstancePerClass());

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldSkipAllCasesWhenShortCircuitingClassExecution()
        {
            Convention.ClassExecution
                .Lifecycle<ShortCircuitClassExecution>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipAllCasesWhenShortCircuitingCaseExecution()
        {
            Convention.ClassExecution
                .Lifecycle<ShortCircuitCaseExection>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(".ctor", ".ctor");
        }

        public void ShouldFailTestRunWhenLifecycleThrows()
        {
            // When a lifecycle throws an exception, either a custom
            // lifecycle directly threw an exception, the test runner
            // encountered an unexpected error, or a listener failed
            // to safely report an event. In all such cases, we're
            // facing a catastrophic failure and need to fail the
            // whole run in order to ensure the problem is reported
            // to the user.

            Convention.ClassExecution.Lifecycle<BuggyLifecycle>();

            Action shouldThrow = () => Run<SampleTestClass>();

            shouldThrow.ShouldThrow<Exception>("Unsafe lifecycle threw!");
        }

        public void ShouldFailTestRunWhenConstructingPerClassAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            Action shouldThrow = () => Run<SampleTestClass>();

            var exception = shouldThrow.ShouldThrow<PreservedException>("Exception of type 'Fixie.PreservedException' was thrown.");
            exception.OriginalException.Message.ShouldEqual("'.ctor' failed!");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndCaseSetUpThrows()
        {
            FailDuring("CaseSetUp");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseSetUp' failed!",
                "SampleTestClass.Fail failed: 'CaseSetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp",
                "CaseSetUp",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndCaseTearDownThrows()
        {
            FailDuring("CaseTearDown");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'CaseTearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndDisposeThrows()
        {
            FailDuring("Dispose");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'Dispose' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Dispose' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldFailTestRunWhenConstructingPerClassAndDisposeThrows()
        {
            FailDuring("Dispose");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            Action shouldThrow = () => Run<SampleTestClass>();

            shouldThrow.ShouldThrow<FailureException>("'Dispose' failed!");
        }

        public void ShouldSkipLifecycleWhenConstructingPerCaseAndAllCasesAreSkipped()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            Convention.CaseExecution.Skip(x => true);

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldNotSkipLifecycleWhenConstructingPerClassAndAllCasesAreSkipped()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            Convention.CaseExecution.Skip(x => true);

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle(".ctor", "Dispose");
        }

        public void ShouldSkipLifecycleWhenConstructingPerCaseButAllCasesFailCustomParameterGeneration()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run<ParameterizedSampleTestClass>();

            output.ShouldHaveResults(
                "ParameterizedSampleTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",
                "ParameterizedSampleTestClass.BoolArg failed: Exception thrown while attempting to yield input parameters for method: BoolArg");

            output.ShouldHaveLifecycle();
        }

        public void ShouldNotSkipLifecycleWhenConstructingPerClassAndAllCasesFailCustomParameterGeneration()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run<ParameterizedSampleTestClass>();

            output.ShouldHaveResults(
                "ParameterizedSampleTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",
                "ParameterizedSampleTestClass.BoolArg failed: Exception thrown while attempting to yield input parameters for method: BoolArg");

            output.ShouldHaveLifecycle(".ctor", "Dispose");
        }

        public void ShouldFailTestRunWhenLifecycleAttemptsToProcessTestCaseLifecycleMultipleTimes()
        {
            Convention.ClassExecution.Lifecycle<RunCasesTwice>();

            Action shouldThrow = () => Run<SampleTestClass>();

            shouldThrow.ShouldThrow<Exception>("Fixie.Tests.LifecycleTests+RunCasesTwice attempted to run Fixie.Tests.LifecycleTests+SampleTestClass's test cases multiple times, which is not supported.");
        }

        public void ShouldAllowExecutingACaseMultipleTimesBeforeEmittingItsResult()
        {
            Convention.ClassExecution.Lifecycle<RetryFailingCases>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail", "Fail");
        }
    }
}