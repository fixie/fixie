using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.TestClasses
{
    public abstract class LifecycleTests
    {
        static string[] FailingMembers;
        protected readonly Convention Convention;

        protected LifecycleTests()
        {
            FailingMembers = null;

            Convention = new Convention();
            Convention.Classes.Where(testClass => testClass == typeof(SampleTestClass));
            Convention.Cases.Where(method => method.Name == "Pass" || method.Name == "Fail");
        }

        protected static void FailDuring(params string[] failingMemberNames)
        {
            FailingMembers = failingMemberNames;
        }

        protected Output Run()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new StubListener();

                Convention.Execute(listener, typeof(SampleTestClass));

                return new Output(console.Lines.ToArray(), listener.Entries.ToArray());
            }
        }

        protected class Output
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
                var namespaceQualifiedExpectation = expected.Select(x => "Fixie.Tests.TestClasses.LifecycleTests+" + x).ToArray();

                results.ShouldEqual(namespaceQualifiedExpectation);
            }
        }

        protected class SampleTestClass : IDisposable
        {
            public SampleTestClass() { WhereAmI(); }
            public void Pass() { WhereAmI(); }
            public void Fail() { WhereAmI(); throw new FailureException(); }
            public void Dispose() { WhereAmI(); }
        }

        protected static void WhereAmI([CallerMemberName] string member = null)
        {
            Console.WriteLine(member);

            if (FailingMembers != null && FailingMembers.Contains(member))
                throw new FailureException(member);
        }
    }

    public class ConstructDisposeLifecycleTests : LifecycleTests
    {
        public void ShouldCreateInstancePerCaseByDefault()
        {
            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowCreatingInstancePerCaseExplicitly()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowCreatingInstancePerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail", "Dispose");
        }

        public void ShouldAllowCreatingInstancePerCaseUsingCustomFactory()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase(Factory);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Factory", ".ctor", "Pass", "Dispose",
                "Factory", ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowCreatingInstancePerTestClassUsingCustomFactory()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass(Factory);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Factory", ".ctor", "Pass", "Fail", "Dispose");
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerCaseAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                ".ctor");
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerTestClassAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(
                ".ctor");
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerCaseAndCustomFactoryThrows()
        {
            FailDuring("Factory");

            Convention.ClassExecution
                      .CreateInstancePerCase(Factory);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'Factory' failed!",
                "SampleTestClass.Fail failed: 'Factory' failed!");

            output.ShouldHaveLifecycle(
                "Factory",
                "Factory");
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerTestClassAndCustomFactoryThrows()
        {
            FailDuring("Factory");

            Convention.ClassExecution
                      .CreateInstancePerTestClass(Factory);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'Factory' failed!",
                "SampleTestClass.Fail failed: 'Factory' failed!");

            output.ShouldHaveLifecycle(
                "Factory");
        }

        static object Factory(Type testClass)
        {
            WhereAmI();
            testClass.ShouldEqual(typeof(SampleTestClass));
            return new SampleTestClass();
        }
    }

    public class ClassLifecycleTests : LifecycleTests
    {
        public void ShouldAllowWrappingClassExecutionWithCustomBehaviors()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap((testClass, conv, cases, innerBehavior) =>
                      {
                          Console.WriteLine("Inner Before");
                          innerBehavior();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((testClass, conv, cases, innerBehavior) =>
                      {
                          Console.WriteLine("Outer Before");
                          innerBehavior();
                          Console.WriteLine("Outer After");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Outer Before", "Inner Before",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "Inner After", "Outer After");
        }

        public void ShouldFailAllCasesWhenClassExecutionCustomBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap((testClass, conv, cases, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe class execution behavior");
                          throw new Exception("Unsafe class execution behavior threw!");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe class execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe class execution behavior threw!");

            output.ShouldHaveLifecycle("Unsafe class execution behavior");
        }

        public void ShouldAllowWrappingClassExecutionWithSetUpTearDownBehaviors()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "SetUp",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "TearDown");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenClassExecutionSetUpThrows()
        {
            FailDuring("SetUp");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'SetUp' failed!",
                "SampleTestClass.Fail failed: 'SetUp' failed!");

            output.ShouldHaveLifecycle(
                "SetUp");
        }

        public void ShouldFailAllCasesWhenClassExecutionTearDownThrows()
        {
            FailDuring("TearDown");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'TearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TearDown' failed!");

            output.ShouldHaveLifecycle(
                "SetUp",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "TearDown");
        }

        static void SetUp(Type testClass)
        {
            testClass.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }

        static void TearDown(Type testClass)
        {
            testClass.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }
    }

    public class InstanceLifecycleTests : LifecycleTests
    {
        public void ShouldAllowWrappingInstanceExecutionWithCustomBehaviors()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .Wrap((fixture, innerBehavior) =>
                      {
                          Console.WriteLine("Inner Before");
                          innerBehavior();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((fixture, innerBehavior) =>
                      {
                          Console.WriteLine("Outer Before");
                          innerBehavior();
                          Console.WriteLine("Outer After");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Outer Before", "Inner Before",
                "Pass", "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenInstanceExecutionCustomBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .Wrap((fixture, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe instance execution behavior");
                          throw new Exception("Unsafe instance execution behavior threw!");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe instance execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe instance execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe instance execution behavior",
                "Dispose");
        }

        public void ShouldAllowWrappingInstanceExecutionWithSetUpTearDownBehaviors()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "Pass", "Fail",
                "TearDown",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenInstanceExecutionSetUpThrows()
        {
            FailDuring("SetUp");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'SetUp' failed!",
                "SampleTestClass.Fail failed: 'SetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenInstanceExecutionTearDownThrows()
        {
            FailDuring("TearDown");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'TearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "Pass", "Fail",
                "TearDown",
                "Dispose");
        }
        
        static void SetUp(Fixture fixture)
        {
            fixture.Cases.Length.ShouldEqual(2);
            fixture.TestClass.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }

        static void TearDown(Fixture fixture)
        {
            fixture.Cases.Length.ShouldEqual(2);
            fixture.TestClass.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }
    }

    public class CaseLifecycleTests : LifecycleTests
    {
        public void ShouldAllowWrappingCaseExecutionWithCustomBehaviors()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .Wrap((@case, instance, innerBehavior) =>
                      {
                          Console.WriteLine("Inner Before");
                          innerBehavior();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((@case, instance, innerBehavior) =>
                      {
                          Console.WriteLine("Outer Before");
                          innerBehavior();
                          Console.WriteLine("Outer After");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Outer Before", "Inner Before",
                "Pass",
                "Inner After", "Outer After",
                "Outer Before", "Inner Before",
                "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenCaseExecutionCustomBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .Wrap((@case, instance, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe case execution behavior");
                          throw new Exception("Unsafe case execution behavior threw!");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe case execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe case execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe case execution behavior",
                "Unsafe case execution behavior",
                "Dispose");
        }

        public void ShouldAllowWrappingCaseExecutionWithSetUpTearDownBehaviors()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp", "Pass", "TearDown",
                "SetUp", "Fail", "TearDown",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenCaseExecutionSetUpThrows()
        {
            FailDuring("SetUp");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'SetUp' failed!",
                "SampleTestClass.Fail failed: 'SetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "SetUp",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenCaseExecutionTearDownThrows()
        {
            FailDuring("TearDown");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'TearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp", "Pass", "TearDown",
                "SetUp", "Fail", "TearDown",
                "Dispose");
        }

        static void SetUp(Case @case, object instance)
        {
            @case.Class.ShouldEqual(typeof(SampleTestClass));
            instance.ShouldBeType<SampleTestClass>();
            WhereAmI();
        }

        static void TearDown(Case @case, object instance)
        {
            @case.Class.ShouldEqual(typeof(SampleTestClass));
            instance.ShouldBeType<SampleTestClass>();
            WhereAmI();
        }
    }
}