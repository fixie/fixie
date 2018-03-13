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

            Convention
                .Classes
                .Where(x => x.Name.EndsWith("TestClass"));

            Convention
                .Methods
                .OrderByDescending(x => x.Name, StringComparer.Ordinal);
        }

        static void FailDuring(params string[] failingMemberNames)
        {
            FailingMembers = failingMemberNames;
        }

        Output Run<TSampleTestClass>()
        {
            return Run(typeof(TSampleTestClass));
        }

        Output Run(Type testClass)
        {
            var listener = new StubListener();

            using (var console = new RedirectedConsole())
            {
                Utility.RunTypes(listener, Convention, testClass);

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

            public void Skip()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void Dispose()
            {
                if (disposed)
                    throw new ShouldBeUnreachableException();
                disposed = true;

                WhereAmI();
            }
        }

        class AllSkippedTestClass : IDisposable
        {
            bool disposed;

            public AllSkippedTestClass()
            {
                WhereAmI();
            }

            public void SkipA()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void SkipB()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void SkipC()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
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

        static class StaticTestClass
        {
            public static void Pass()
            {
                WhereAmI();
            }

            public static void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public static void Skip()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
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
                    if (@case.Method.Name.Contains("Skip"))
                        return;

                    var instance = testClass.Construct();

                    @case.Execute(instance);

                    instance.Dispose();
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
                    if (@case.Method.Name.Contains("Skip"))
                    {
                        @case.Skip("skipped by naming convention");
                        return;
                    }

                    CaseSetUp(@case);
                    @case.Execute(instance);
                    CaseTearDown(@case);
                });

                instance.Dispose();
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

                for (int i = 1; i <= 2; i++)
                {
                    runCases(@case =>
                    {
                        if (@case.Method.Name.Contains("Skip"))
                            return;

                        @case.Execute(instance);
                    });
                }

                instance.Dispose();
            }
        }

        class RetryFailingCases : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                var instance = testClass.Construct();

                runCases(@case =>
                {
                    if (@case.Method.Name.Contains("Skip"))
                        return;

                    @case.Execute(instance);

                    if (@case.Exception != null)
                        @case.Execute(instance);
                });

                instance.Dispose();
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
            Convention.Methods.Where(x => x.Name != "Skip");

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
            Convention.Lifecycle<CreateInstancePerCase>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerClassUsingLifecycleType()
        {
            Convention.Lifecycle<CreateInstancePerClass>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped: skipped by naming convention",
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
            Convention.Lifecycle(new CreateInstancePerCase());

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerClassUsingLifecycleInstance()
        {
            Convention.Lifecycle(new CreateInstancePerClass());

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped: skipped by naming convention",
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
            Convention.Lifecycle<ShortCircuitClassExecution>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipAllCasesWhenShortCircuitingCaseExecution()
        {
            Convention.Lifecycle<ShortCircuitCaseExection>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.Lifecycle<CreateInstancePerCase>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(".ctor", ".ctor");
        }

        public void ShouldFailAllMethodsWhenLifecycleThrows()
        {
            Convention.Lifecycle<BuggyLifecycle>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip failed: Unsafe lifecycle threw!",
                "SampleTestClass.Pass failed: Unsafe lifecycle threw!",
                "SampleTestClass.Fail failed: Unsafe lifecycle threw!");

            output.ShouldHaveLifecycle();
        }

        public void ShouldFailAllMethodsWhenConstructingPerClassAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.Lifecycle<CreateInstancePerClass>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip failed: '.ctor' failed!",
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(".ctor");
        }

        public void ShouldFailCasesWhenConstructingPerClassAndCaseSetUpThrows()
        {
            FailDuring("CaseSetUp");

            Convention.Lifecycle<CreateInstancePerClass>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped: skipped by naming convention",
                "SampleTestClass.Pass failed: 'CaseSetUp' failed!",
                "SampleTestClass.Fail failed: 'CaseSetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp",
                "CaseSetUp",
                "Dispose");
        }

        public void ShouldFailCasesWhenConstructingPerClassAndCaseTearDownThrows()
        {
            FailDuring("CaseTearDown");

            Convention.Lifecycle<CreateInstancePerClass>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped: skipped by naming convention",
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

            Convention.Lifecycle<CreateInstancePerCase>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Pass failed: 'Dispose' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Dispose' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldFailAllMethodsAfterReportingPrimaryResultsWhenConstructingPerClassAndDisposeThrows()
        {
            FailDuring("Dispose");

            Convention.Lifecycle<CreateInstancePerClass>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped: skipped by naming convention",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!",

                "SampleTestClass.Skip failed: 'Dispose' failed!",
                "SampleTestClass.Pass failed: 'Dispose' failed!",
                "SampleTestClass.Fail failed: 'Dispose' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldSkipLifecycleWhenConstructingPerCaseAndAllCasesAreSkipped()
        {
            Convention.Lifecycle<CreateInstancePerCase>();

            var output = Run<AllSkippedTestClass>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipC skipped",
                "AllSkippedTestClass.SkipB skipped",
                "AllSkippedTestClass.SkipA skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldNotSkipLifecycleWhenConstructingPerClassAndAllCasesAreSkipped()
        {
            Convention.Lifecycle<CreateInstancePerClass>();

            var output = Run<AllSkippedTestClass>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipC skipped: skipped by naming convention",
                "AllSkippedTestClass.SkipB skipped: skipped by naming convention",
                "AllSkippedTestClass.SkipA skipped: skipped by naming convention");

            output.ShouldHaveLifecycle(".ctor", "Dispose");
        }

        public void ShouldSkipLifecycleWhenConstructingPerCaseButAllCasesFailCustomParameterGeneration()
        {
            Convention.Lifecycle<CreateInstancePerCase>();

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run<ParameterizedSampleTestClass>();

            output.ShouldHaveResults(
                "ParameterizedSampleTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",
                "ParameterizedSampleTestClass.BoolArg failed: Exception thrown while attempting to yield input parameters for method: BoolArg");

            output.ShouldHaveLifecycle();
        }

        public void ShouldNotSkipLifecycleWhenConstructingPerClassAndAllCasesFailCustomParameterGeneration()
        {
            Convention.Lifecycle<CreateInstancePerClass>();

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run<ParameterizedSampleTestClass>();

            output.ShouldHaveResults(
                "ParameterizedSampleTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",
                "ParameterizedSampleTestClass.BoolArg failed: Exception thrown while attempting to yield input parameters for method: BoolArg");

            output.ShouldHaveLifecycle(".ctor", "Dispose");
        }

        public void ShouldAllowRunningAllCasesMultipleTimes()
        {
            Convention.Lifecycle<RunCasesTwice>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!",

                "SampleTestClass.Skip skipped",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail", "Pass", "Fail", "Dispose");
        }

        public void ShouldAllowExecutingACaseMultipleTimesBeforeEmittingItsResult()
        {
            Convention.Lifecycle<RetryFailingCases>();

            var output = Run<SampleTestClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail", "Fail", "Dispose");
        }

        public void ShouldAllowStaticTestClassesAndMethodsBypassingConstructionAttempts()
        {
            Convention.Lifecycle<CreateInstancePerCase>();

            var output = Run(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Skip skipped",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Pass", "Fail");
        }
    }
}