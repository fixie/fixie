namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Internal;

    public class LifecycleTests
    {
        static string[] FailingMembers = Array.Empty<string>();

        readonly Discovery discovery;

        public LifecycleTests()
        {
            FailingMembers = Array.Empty<string>();
            discovery = new SelfTestDiscovery();
        }

        static void FailDuring(params string[] failingMemberNames)
        {
            FailingMembers = failingMemberNames;
        }

        Output Run<TSampleTestClass, TExecution>() where TExecution : Execution, new()
        {
            return Run<TExecution>(typeof(TSampleTestClass));
        }

        Output Run<TExecution>(Type testClass) where TExecution : Execution, new()
        {
            var listener = new StubListener();
            var execution = new TExecution();
            using var console = new RedirectedConsole();

            Utility.Run(listener, discovery, execution, testClass);

            return new Output(console.Lines().ToArray(), listener.Entries.ToArray());
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
                lifecycle.ShouldBe(expected);
            }

            public void ShouldHaveResults(params string[] expected)
            {
                var namespaceQualifiedExpectation = expected.Select(x => "Fixie.Tests.LifecycleTests+" + x).ToArray();

                results.ShouldBe(namespaceQualifiedExpectation);
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

        static void WhereAmI([CallerMemberName] string member = default!)
        {
            System.Console.WriteLine(member);

            if (FailingMembers.Contains(member))
                throw new FailureException(member);
        }

        class CreateInstancePerCase : Execution
        {
            public void Execute(TestClass testClass)
            {
                testClass.RunTests(test =>
                {
                    if (test.Method.Name.Contains("Skip"))
                        return;

                    TestSetUp(test);
                    test.RunCases(@case =>
                    {
                        var instance = testClass.Construct();

                        @case.Execute(instance);

                        instance.Dispose();
                    });
                    TestTearDown(test);
                });
            }

            static void TestSetUp(TestMethod test) => WhereAmI();
            static void TestTearDown(TestMethod test) => WhereAmI();
        }

        class CreateInstancePerClass : Execution
        {
            public void Execute(TestClass testClass)
            {
                var instance = testClass.Construct();

                testClass.RunTests(test =>
                {
                    if (test.Method.Name.Contains("Skip"))
                        return;

                    TestSetUp(test);
                    test.RunCases(@case =>
                    {
                        CaseSetUp(@case);
                        @case.Execute(instance);
                        CaseTearDown(@case);
                    });
                    TestTearDown(test);
                });

                instance.Dispose();
            }

            static void TestSetUp(TestMethod test) => WhereAmI();
            static void CaseSetUp(Case @case) => WhereAmI();
            static void CaseTearDown(Case @case) => WhereAmI();
            static void TestTearDown(TestMethod test) => WhereAmI();
        }

        class BuggyExecution : Execution
        {
            public void Execute(TestClass testClass)
                => throw new Exception("Unsafe custom execution threw!");
        }

        class ShortCircuitClassExecution : Execution
        {
            public void Execute(TestClass testClass)
            {
                //Class lifecycle chooses not to invoke testClass.RunTests(...).
                //Since the tests never run, they are all considered
                //'skipped'.
            }
        }

        class ShortCircuitTestExection : Execution
        {
            public void Execute(TestClass testClass)
            {
                testClass.RunTests(test =>
                {
                    //Test lifecycle chooses not to invoke test.RunCases(...).
                    //Since the tests never run, they are all considered
                    //'skipped'.
                });
            }
        }

        class ShortCircuitCaseExection : Execution
        {
            public void Execute(TestClass testClass)
            {
                testClass.RunTests(test =>
                {
                    test.RunCases(@case =>
                    {
                        //Case lifecycle chooses not to invoke @case.Execute(instance).
                        //Since the test cases never run, they are all considered
                        //'skipped'.
                    });
                });
            }
        }

        class RunCasesTwice : Execution
        {
            public void Execute(TestClass testClass)
            {
                var instance = testClass.Construct();

                for (int i = 1; i <= 2; i++)
                {
                    testClass.RunTests(test =>
                    {
                        if (test.Method.Name.Contains("Skip"))
                            return;

                        test.RunCases(@case =>
                        {
                            @case.Execute(instance);
                        });
                    });
                }

                instance.Dispose();
            }
        }

        class RetryFailingCases : Execution
        {
            public void Execute(TestClass testClass)
            {
                var instance = testClass.Construct();

                testClass.RunTests(test =>
                {
                    if (test.Method.Name.Contains("Skip"))
                        return;

                    test.RunCases(@case =>
                    {
                        @case.Execute(instance);

                        if (@case.Exception != null)
                            @case.Execute(instance);
                    });
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
            discovery.Methods.Where(x => x.Name != "Skip");

            var output = Run<SampleTestClass, DefaultExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "Dispose",
                ".ctor", "Pass", "Dispose");
        }

        public void ShouldAllowConstructingPerCase()
        {
            var output = Run<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "TestSetUp", ".ctor", "Fail", "Dispose", "TestTearDown",
                "TestSetUp", ".ctor", "Pass", "Dispose", "TestTearDown");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndTestSetUpThrows()
        {
            FailDuring("TestSetUp");

            var output = Run<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'TestSetUp' failed!",
                "SampleTestClass.Pass failed: 'TestSetUp' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle("TestSetUp", "TestSetUp");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndConstructorThrows()
        {
            FailDuring(".ctor");

            var output = Run<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: '.ctor' failed!",
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "TestSetUp", ".ctor", "TestTearDown",
                "TestSetUp", ".ctor", "TestTearDown");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndDisposeThrows()
        {
            FailDuring("Dispose");

            var output = Run<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Dispose' failed!",
                "SampleTestClass.Pass failed: 'Dispose' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "TestSetUp", ".ctor", "Fail", "Dispose", "TestTearDown",
                "TestSetUp", ".ctor", "Pass", "Dispose", "TestTearDown");
        }

        public void ShouldFailCasesAlongsidePrimaryResultsWhenConstructingPerCaseAndTestTearDownThrows()
        {
            FailDuring("TestTearDown");

            var output = Run<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'TestTearDown' failed!",

                "SampleTestClass.Pass passed",
                "SampleTestClass.Pass failed: 'TestTearDown' failed!",
                
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "TestSetUp", ".ctor", "Fail", "Dispose", "TestTearDown",
                "TestSetUp", ".ctor", "Pass", "Dispose", "TestTearDown");
        }

        public void ShouldSkipLifecycleWhenConstructingPerCaseAndAllCasesAreSkipped()
        {
            var output = Run<AllSkippedTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped",
                "AllSkippedTestClass.SkipB skipped",
                "AllSkippedTestClass.SkipC skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipLifecycleWhenConstructingPerCaseButAllCasesFailCustomParameterGeneration()
        {
            discovery.Parameters.Add<BuggyParameterSource>();

            var output = Run<ParameterizedSampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "ParameterizedSampleTestClass.BoolArg failed: Exception thrown while attempting to yield input parameters for method: BoolArg",
                "ParameterizedSampleTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg");

            output.ShouldHaveLifecycle("TestSetUp", "TestSetUp");
        }

        public void ShouldAllowConstructingPerClass()
        {
            var output = Run<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor",
                "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
                "TestSetUp", "CaseSetUp", "Pass", "CaseTearDown", "TestTearDown",
                "Dispose");
        }

        public void ShouldFailAllMethodsWhenConstructingPerClassAndConstructorThrows()
        {
            FailDuring(".ctor");

            var output = Run<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: '.ctor' failed!",
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Skip failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(".ctor");
        }

        public void ShouldFailCasesWhenConstructingPerClassAndTestSetUpThrows()
        {
            FailDuring("TestSetUp");

            var output = Run<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'TestSetUp' failed!",
                "SampleTestClass.Pass failed: 'TestSetUp' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor",
                "TestSetUp",
                "TestSetUp",
                "Dispose");
        }

        public void ShouldFailCasesWhenConstructingPerClassAndCaseSetUpThrows()
        {
            FailDuring("CaseSetUp");

            var output = Run<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'CaseSetUp' failed!",
                "SampleTestClass.Pass failed: 'CaseSetUp' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor",
                "TestSetUp", "CaseSetUp", "TestTearDown",
                "TestSetUp", "CaseSetUp", "TestTearDown",
                "Dispose");
        }

        public void ShouldFailCasesWhenConstructingPerClassAndCaseTearDownThrows()
        {
            FailDuring("CaseTearDown");

            var output = Run<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'CaseTearDown' failed!",
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor",
                "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
                "TestSetUp", "CaseSetUp", "Pass", "CaseTearDown", "TestTearDown",
                "Dispose");
        }

        public void ShouldFailCasesAlongsidePrimaryResultsWhenConstructingPerClassAndTestTearDownThrows()
        {
            FailDuring("TestTearDown");

            var output = Run<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'TestTearDown' failed!",
                
                "SampleTestClass.Pass passed",
                "SampleTestClass.Pass failed: 'TestTearDown' failed!",
                
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor",
                "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
                "TestSetUp", "CaseSetUp", "Pass", "CaseTearDown", "TestTearDown",
                "Dispose");
        }

        public void ShouldFailAllMethodsAfterReportingPrimaryResultsWhenConstructingPerClassAndDisposeThrows()
        {
            FailDuring("Dispose");

            var output = Run<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip skipped",

                "SampleTestClass.Fail failed: 'Dispose' failed!",
                "SampleTestClass.Pass failed: 'Dispose' failed!",
                "SampleTestClass.Skip failed: 'Dispose' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
                "TestSetUp", "CaseSetUp", "Pass", "CaseTearDown", "TestTearDown",
                "Dispose");
        }

        public void ShouldNotSkipLifecycleWhenConstructingPerClassAndAllCasesAreSkipped()
        {
            var output = Run<AllSkippedTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped",
                "AllSkippedTestClass.SkipB skipped",
                "AllSkippedTestClass.SkipC skipped");

            output.ShouldHaveLifecycle(".ctor", "Dispose");
        }

        public void ShouldNotSkipLifecycleWhenConstructingPerClassAndAllCasesFailCustomParameterGeneration()
        {
            discovery.Parameters.Add<BuggyParameterSource>();

            var output = Run<ParameterizedSampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "ParameterizedSampleTestClass.BoolArg failed: Exception thrown while attempting to yield input parameters for method: BoolArg",
                "ParameterizedSampleTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg");

            output.ShouldHaveLifecycle(".ctor", "TestSetUp", "TestSetUp", "Dispose");
        }

        public void ShouldAllowRunningAllCasesMultipleTimes()
        {
            var output = Run<SampleTestClass, RunCasesTwice>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip skipped",

                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "Pass", "Fail", "Pass", "Dispose");
        }

        public void ShouldAllowExecutingACaseMultipleTimesBeforeEmittingItsResult()
        {
            var output = Run<SampleTestClass, RetryFailingCases>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "Fail", "Pass", "Dispose");
        }

        public void ShouldAllowStaticTestClassesAndMethodsBypassingConstructionAttempts()
        {
            var output = Run<CreateInstancePerCase>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "TestSetUp", "Fail", "TestTearDown",
                "TestSetUp", "Pass", "TestTearDown");
        }

        public void ShouldSkipAllTestsWhenShortCircuitingClassExecution()
        {
            var output = Run<SampleTestClass, ShortCircuitClassExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipAllTestsWhenShortCircuitingTestExecution()
        {
            var output = Run<SampleTestClass, ShortCircuitTestExection>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipAllCasesWhenShortCircuitingCaseExecution()
        {
            var output = Run<SampleTestClass, ShortCircuitCaseExection>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldFailAllMethodsWhenCustomExecutionThrows()
        {
            var output = Run<SampleTestClass, BuggyExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: Unsafe custom execution threw!",
                "SampleTestClass.Pass failed: Unsafe custom execution threw!",
                "SampleTestClass.Skip failed: Unsafe custom execution threw!");

            output.ShouldHaveLifecycle();
        }
    }
}