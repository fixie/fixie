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
            Convention.ClassExecution.SortCases((x, y) => String.Compare(y.Name, x.Name, StringComparison.Ordinal));
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

        public void ShouldAllowConstructingPerCaseUsingLifecycleAction()
        {
            Convention.ClassExecution
                .Lifecycle((testClass, runCases) =>
                {
                    runCases(@case =>
                    {
                        var instance = Activator.CreateInstance(testClass);

                        @case.Execute(instance);

                        (instance as IDisposable)?.Dispose();
                    });
                });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerClassUsingLifecycleAction()
        {
            Convention.ClassExecution
                .Lifecycle((testClass, runCases) =>
                {
                    var instance = Activator.CreateInstance(testClass);

                    runCases(@case =>
                    {
                        @case.Execute(instance);
                    });

                    (instance as IDisposable)?.Dispose();
                });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail", "Dispose");
        }

        public void ShouldAllowShortCircuitingCasesExecution()
        {
            Convention.ClassExecution.Lifecycle((testClass, runCases) =>
            {
                //Behavior chooses not to invoke runCases(...).
                //Since the test cases never run, they don't have
                //a chance to throw exceptions, resulting in all
                //'passing'.
            });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail passed");

            output.ShouldHaveLifecycle();
        }

        public void ShouldAllowShortCircuitingCaseExecution()
        {
            Convention.ClassExecution.Lifecycle((testClass, runCases) =>
            {
                runCases(@case =>
                {
                    //Behavior chooses not to invoke @case.Execute(instance).
                    //Since the test cases never run, they don't have
                    //a chance to throw exceptions, resulting in all
                    //'passing'.
                });
            });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail passed");

            output.ShouldHaveLifecycle();
        }

        public void ShouldFailAllCasesWhenLifecycleThrows()
        {
            Convention.ClassExecution.Lifecycle((testClass, runCases) =>
            {
                Console.WriteLine("Unsafe class lifecycle");
                throw new Exception("Unsafe class lifecycle threw!");
            });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe class lifecycle threw!",
                "SampleTestClass.Fail failed: Unsafe class lifecycle threw!");

            output.ShouldHaveLifecycle("Unsafe class lifecycle");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenLifecycleThrowsPreservedException()
        {
            Convention.ClassExecution.Lifecycle((testClass, runCases) =>
            {
                Console.WriteLine("Unsafe class lifecycle");
                try
                {
                    throw new Exception("Unsafe class lifecycle threw!");
                }
                catch (Exception originalException)
                {
                    throw new PreservedException(originalException);
                }
            });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe class lifecycle threw!",
                "SampleTestClass.Fail failed: Unsafe class lifecycle threw!");

            output.ShouldHaveLifecycle("Unsafe class lifecycle");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndCaseLifecycleThrows()
        {
            Convention.ClassExecution.Lifecycle((testClass, runCases) =>
            {
                runCases(@case =>
                {
                    Console.WriteLine("Unsafe case lifecycle");
                    throw new Exception("Unsafe case lifecycle threw!");
                });
            });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe case lifecycle threw!",
                "SampleTestClass.Fail failed: Unsafe case lifecycle threw!");

            output.ShouldHaveLifecycle(
                "Unsafe case lifecycle",
                "Unsafe case lifecycle");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndCaseLifecycleThrows()
        {
            Convention.ClassExecution.Lifecycle((testClass, runCases) =>
            {
                var instance = Activator.CreateInstance(testClass);

                runCases(@case =>
                {
                    Console.WriteLine("Unsafe case lifecycle");
                    throw new Exception("Unsafe case lifecycle threw!");
                });

                (instance as IDisposable)?.Dispose();
            });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe case lifecycle threw!",
                "SampleTestClass.Fail failed: Unsafe case lifecycle threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe case lifecycle",
                "Unsafe case lifecycle",
                "Dispose");
        }

        public void ShouldFailCaseWithOriginalExceptionWhenConstructingPerCaseAndCaseBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution.Lifecycle((testClass, runCases) =>
            {
                runCases(@case =>
                {
                    Console.WriteLine("Unsafe case lifecycle");
                    try
                    {
                        throw new Exception("Unsafe case lifecycle threw!");
                    }
                    catch (Exception originalException)
                    {
                        throw new PreservedException(originalException);
                    }
                });
            });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe case lifecycle threw!",
                "SampleTestClass.Fail failed: Unsafe case lifecycle threw!");

            output.ShouldHaveLifecycle(
                "Unsafe case lifecycle",
                "Unsafe case lifecycle");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenConstructingPerClassAndCaseBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution.Lifecycle((testClass, runCases) =>
            {
                var instance = Activator.CreateInstance(testClass);

                runCases(@case =>
                {
                    Console.WriteLine("Unsafe case lifecycle");
                    try
                    {
                        throw new Exception("Unsafe case lifecycle threw!");
                    }
                    catch (Exception originalException)
                    {
                        throw new PreservedException(originalException);
                    }
                });

                (instance as IDisposable)?.Dispose();
            });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe case lifecycle threw!",
                "SampleTestClass.Fail failed: Unsafe case lifecycle threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe case lifecycle",
                "Unsafe case lifecycle",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenConstructingPerCaseAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!");

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

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerClassAndCaseSetUpThrows()
        {
            FailDuring("CaseSetUp");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

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

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldSkipConstructingPerCaseWhenAllCasesSkipped()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            Convention.CaseExecution.Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerClassWhenAllCasesSkipped()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            Convention.CaseExecution.Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerCaseWhenAllCasesFailCustomParameterGeneration()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Exception thrown while attempting to yield input parameters for method: Pass",
                "SampleTestClass.Fail failed: Exception thrown while attempting to yield input parameters for method: Fail");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerClassWhenAllCasesFailCustomParameterGeneration()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Exception thrown while attempting to yield input parameters for method: Pass",
                "SampleTestClass.Fail failed: Exception thrown while attempting to yield input parameters for method: Fail");

            output.ShouldHaveLifecycle();
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndDisposeThrows()
        {
            FailDuring("Dispose");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'Dispose' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'Dispose' failed!");

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
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'Dispose' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldIncludeAllPossibleTearDownExceptionsInResultWhenConstructingPerClass()
        {
            FailDuring("CaseTearDown", "Dispose");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + NewLine +
                "    Secondary Failure: 'Dispose' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!" + NewLine +
                "    Secondary Failure: 'Dispose' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",

                "CaseSetUp",
                "Pass",
                "CaseTearDown",

                "CaseSetUp",
                "Fail",
                "CaseTearDown",

                "Dispose");
        }

        public void ShouldAllowProcessingTestCaseLifecycleMultipleTimes()
        {
            Convention.ClassExecution
                .Lifecycle((testClass, runCases) =>
                {
                    var instance = Activator.CreateInstance(testClass);

                    runCases(@case => @case.Execute(instance));
                    runCases(@case => @case.Execute(instance));
                });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail", "Pass", "Fail");
        }
    }
}