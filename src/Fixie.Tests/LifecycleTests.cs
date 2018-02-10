namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Execution;
    using static System.Environment;

    public class LifecycleTests
    {
        static string[] FailingMembers;
        readonly Convention Convention;

        public LifecycleTests()
        {
            FailingMembers = null;

            Convention = new Convention();
            Convention.Classes.Where(testClass => testClass == typeof(SampleTestClass));
            Convention.Methods.Where(method => method.Name == "Pass" || method.Name == "Fail");
            Convention.ClassExecution.SortMethods((x, y) => String.Compare(y.Name, x.Name, StringComparison.Ordinal));
        }

        static void FailDuring(params string[] failingMemberNames)
        {
            FailingMembers = failingMemberNames;
        }

        Output Run()
        {
            var listener = new StubListener();

            using (var console = new RedirectedConsole())
            {
                Utility.Run<SampleTestClass>(listener, Convention);

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

        static void WhereAmI([CallerMemberName] string member = null)
        {
            Console.WriteLine(member);

            if (FailingMembers != null && FailingMembers.Contains(member))
                throw new FailureException(member);
        }

        class CreateInstancePerCase : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                runCases(@case =>
                {
                    var instance = UseDefaultConstructor(testClass);

                    @case.Execute(instance);

                    (instance as IDisposable)?.Dispose();
                });
            }
        }

        class CreateInstancePerClass : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                var instance = UseDefaultConstructor(testClass);

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

        class ShortCircuitClassExecution : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                //Class lifecycle chooses not to invoke runCases(...).
                //Since the test cases never run, they are all considered
                //'skipped'.
            }
        }

        class ShortCircuitCaseExection : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
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
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                var instance = Activator.CreateInstance(testClass);

                runCases(@case => @case.Execute(instance));
                runCases(@case => @case.Execute(instance));
            }
        }

        static object UseDefaultConstructor(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception.InnerException);
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
            var output = Run();

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

            var output = Run();

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

            var output = Run();

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

            var output = Run();

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

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldPassAllCasesWhenShortCircuitingClassExecution()
        {
            Convention.ClassExecution
                .Lifecycle<ShortCircuitClassExecution>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldPassAllCasesWhenShortCircuitingCaseExecution()
        {
            Convention.ClassExecution
                .Lifecycle<ShortCircuitCaseExection>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle(".ctor", ".ctor");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(".ctor");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndCaseSetUpThrows()
        {
            FailDuring("CaseSetUp");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseSetUp' failed!",
                "SampleTestClass.Fail failed: 'CaseSetUp' failed!",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

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

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!",
                "SampleTestClass.Fail failed: 'CaseTearDown' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

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

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'Dispose' failed!",
                "SampleTestClass.Fail failed: 'Dispose' failed!",

                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndDisposeThrows()
        {
            FailDuring("Dispose");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'Dispose' failed!",
                "SampleTestClass.Fail failed: 'Dispose' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldSkipLifecycleWhenConstructingPerCaseButAllCasesAreSkipped()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            Convention.CaseExecution.Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipLifecycleWhenConstructingPerClassButAllCasesAreSkipped()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            Convention.CaseExecution.Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle(".ctor", "Dispose");
        }

        public void ShouldSkipLifecycleWhenConstructingPerCaseButAllCasesFailCustomParameterGeneration()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Exception thrown while attempting to yield input parameters for method: Pass",
                "SampleTestClass.Fail failed: Exception thrown while attempting to yield input parameters for method: Fail");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipLifecycleWhenConstructingPerClassButAllCasesFailCustomParameterGeneration()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Exception thrown while attempting to yield input parameters for method: Pass",
                "SampleTestClass.Fail failed: Exception thrown while attempting to yield input parameters for method: Fail");

            output.ShouldHaveLifecycle(".ctor", "Dispose");
        }

        public void ShouldDisallowProcessingTestCaseLifecycleMultipleTimes()
        {
            Convention.ClassExecution.Lifecycle<RunCasesTwice>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Fixie.Tests.LifecycleTests+RunCasesTwice attempted to run Fixie.Tests.LifecycleTests+SampleTestClass's test cases multiple times, which is not supported.",
                "SampleTestClass.Fail failed: Fixie.Tests.LifecycleTests+RunCasesTwice attempted to run Fixie.Tests.LifecycleTests+SampleTestClass's test cases multiple times, which is not supported.",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail");
        }
    }
}