namespace Fixie.Tests.Lifecycle
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Assertions;
    using Lifecycle = Fixie.Lifecycle;
    using static System.Environment;

    public class LifecycleTests : BaseLifecycleTests
    {
        static void ClassSetUp(Type testClass)
        {
            testClass.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }

        static void ClassTearDown(Type testClass)
        {
            testClass.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }

        static void InstanceSetUp(object instance)
        {
            instance.GetType().ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }

        static void InstanceTearDown(object instance)
        {
            instance.GetType().ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }

        new static void CaseSetUp(Case @case)
        {
            @case.Class.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }

        new static void CaseTearDown(Case @case)
        {
            @case.Class.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }

        class CreateInstancePerCase : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                ClassSetUp(testClass);

                runCases(@case =>
                {
                    var instance = Activator.CreateInstance(testClass);

                    InstanceSetUp(instance);

                    CaseSetUp(@case);
                    @case.Execute(instance);

                    try
                    {
                        CaseTearDown(@case);
                    }
                    catch (Exception exception)
                    {
                        @case.Fail(exception);
                    }

                    try
                    {
                        InstanceTearDown(instance);
                    }
                    catch (Exception exception)
                    {
                        @case.Fail(exception);
                    }

                    (instance as IDisposable)?.Dispose();
                });

                ClassTearDown(testClass);
            }
        }

        class CreateInstancePerClass : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                ClassSetUp(testClass);

                try
                {
                    var instance = Activator.CreateInstance(testClass);

                    InstanceSetUp(instance);

                    runCases(@case =>
                    {
                        CaseSetUp(@case);
                        @case.Execute(instance);
                        CaseTearDown(@case);
                    });

                    try
                    {
                        InstanceTearDown(instance);
                    }
                    finally
                    {
                        (instance as IDisposable)?.Dispose();
                    }
                }
                finally
                {
                    ClassTearDown(testClass);
                }
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
                "ClassSetUp",
                ".ctor", "InstanceSetUp", "CaseSetUp", "Pass", "CaseTearDown", "InstanceTearDown", "Dispose",
                ".ctor", "InstanceSetUp", "CaseSetUp", "Fail", "CaseTearDown", "InstanceTearDown", "Dispose",
                "ClassTearDown");
        }

        public void ShouldAllowConstructingPerClassUsingLifecycleType()
        {
            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor", "InstanceSetUp",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "InstanceTearDown", "Dispose",
                "ClassTearDown");
        }

        public void ShouldAllowConstructingPerCaseUsingLifecycleInstance()
        {
            Convention.ClassExecution.Lifecycle(new CreateInstancePerCase());

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor", "InstanceSetUp", "CaseSetUp", "Pass", "CaseTearDown", "InstanceTearDown", "Dispose",
                ".ctor", "InstanceSetUp", "CaseSetUp", "Fail", "CaseTearDown", "InstanceTearDown", "Dispose",
                "ClassTearDown");
        }

        public void ShouldAllowConstructingPerClassUsingLifecycleInstance()
        {
            Convention.ClassExecution.Lifecycle(new CreateInstancePerClass());

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor", "InstanceSetUp",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "InstanceTearDown", "Dispose",
                "ClassTearDown");
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

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingCasesWhenConstructingPerCaseAndClassSetUpThrows()
        {
            FailDuring("ClassSetUp");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'ClassSetUp' failed!",
                "SampleTestClass.Fail failed: 'ClassSetUp' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerClassAndClassSetUpThrows()
        {
            FailDuring("ClassSetUp");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'ClassSetUp' failed!",
                "SampleTestClass.Fail failed: 'ClassSetUp' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingCaseWhenConstructingPerCaseAndInstanceSetUpThrows()
        {
            FailDuring("InstanceSetUp");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'InstanceSetUp' failed!",
                "SampleTestClass.Fail failed: 'InstanceSetUp' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",
                "InstanceSetUp",
                ".ctor",
                "InstanceSetUp",
                "ClassTearDown");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerClassAndInstanceSetUpThrows()
        {
            FailDuring("InstanceSetUp");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'InstanceSetUp' failed!",
                "SampleTestClass.Fail failed: 'InstanceSetUp' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",
                "InstanceSetUp",
                "ClassTearDown");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndInstanceTearDownThrows()
        {
            FailDuring("InstanceTearDown");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'InstanceTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'InstanceTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp", "Pass", "CaseTearDown",
                "InstanceTearDown",
                "Dispose",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp", "Fail", "CaseTearDown",
                "InstanceTearDown",
                "Dispose",
                "ClassTearDown");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndInstanceTearDownThrows()
        {
            FailDuring("InstanceTearDown");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'InstanceTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'InstanceTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "InstanceTearDown",
                "Dispose",
                "ClassTearDown");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndClassTearDownThrows()
        {
            FailDuring("ClassTearDown");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor", "InstanceSetUp", "CaseSetUp", "Pass", "CaseTearDown", "InstanceTearDown", "Dispose",
                ".ctor", "InstanceSetUp", "CaseSetUp", "Fail", "CaseTearDown", "InstanceTearDown", "Dispose",
                "ClassTearDown");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndClassTearDownThrows()
        {
            FailDuring("ClassTearDown");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "InstanceTearDown",
                "Dispose",
                "ClassTearDown");
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

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingCaseWhenConstructingPerCaseAndCaseSetUpThrows()
        {
            FailDuring("CaseSetUp");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseSetUp' failed!",
                "SampleTestClass.Fail failed: 'CaseSetUp' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp",
                "ClassTearDown");
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
                "ClassSetUp",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp",
                "CaseSetUp",
                "InstanceTearDown",
                "Dispose",
                "ClassTearDown");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndCaseTearDownThrows()
        {
            FailDuring("CaseTearDown");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp", "Pass", "CaseTearDown",
                "InstanceTearDown",
                "Dispose",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp", "Fail", "CaseTearDown",
                "InstanceTearDown",
                "Dispose",
                "ClassTearDown");
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
                "ClassSetUp",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "InstanceTearDown",
                "Dispose",
                "ClassTearDown");
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
                "ClassSetUp",
                ".ctor", "InstanceSetUp", "CaseSetUp", "Pass", "CaseTearDown", "InstanceTearDown", "Dispose",
                ".ctor", "InstanceSetUp", "CaseSetUp", "Fail", "CaseTearDown", "InstanceTearDown", "Dispose",
                "ClassTearDown");
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
                "ClassSetUp",
                ".ctor",
                "InstanceSetUp",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "InstanceTearDown",
                "Dispose",
                "ClassTearDown");
        }

        public void ShouldIncludeAllTearDownAndDisposalExceptionsInResultWhenConstructingPerCase()
        {
            FailDuring("ClassTearDown", "InstanceTearDown", "CaseTearDown", "Dispose");

            Convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + NewLine +
                "    Secondary Failure: 'InstanceTearDown' failed!" + NewLine +
                "    Secondary Failure: 'Dispose' failed!" + NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!" + NewLine +
                "    Secondary Failure: 'InstanceTearDown' failed!" + NewLine +
                "    Secondary Failure: 'Dispose' failed!" + NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",

                ".ctor",
                "InstanceSetUp",
                "CaseSetUp",
                "Pass",
                "CaseTearDown",
                "InstanceTearDown",
                "Dispose",

                ".ctor",
                "InstanceSetUp",
                "CaseSetUp",
                "Fail",
                "CaseTearDown",
                "InstanceTearDown",
                "Dispose",

                "ClassTearDown");
        }

        public void ShouldIncludeAllPossibleTearDownExceptionsInResultWhenConstructingPerClass()
        {
            FailDuring("ClassTearDown", "InstanceTearDown", "CaseTearDown", "Dispose");

            Convention.ClassExecution.Lifecycle<CreateInstancePerClass>();

            var output = Run();

            //Instance-per-class can accurately describe the first failure,
            //but secondary failures InstanceTearDown and Dispose will not
            //be included in the compound stack trace. Still, all tear downs
            //and disposals can be invoked even during catastrophic failures.

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!" + NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",

                "InstanceSetUp",
                "CaseSetUp",
                "Pass",
                "CaseTearDown",

                "CaseSetUp",
                "Fail",
                "CaseTearDown",
                "InstanceTearDown",

                "Dispose",
                "ClassTearDown");
        }
    }
}