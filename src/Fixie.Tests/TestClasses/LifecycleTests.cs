using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.TestClasses
{
    public class LifecycleTests
    {
        static string[] FailingMembers;
        readonly Convention convention;

        public LifecycleTests()
        {
            FailingMembers = null;
            
            convention = new Convention();
            convention.Classes.Where(testClass => testClass == typeof(SampleTestClass));
            convention.Cases.Where(method => method.Name == "Pass" || method.Name == "Fail");
        }

        public void ShouldCreateInstancePerCaseByDefault()
        {
            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowCreatingInstancePerCaseExplicitly()
        {
            convention.ClassExecution
                      .CreateInstancePerCase();

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowCreatingInstancePerTestClass()
        {
            convention.ClassExecution
                      .CreateInstancePerTestClass();

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail", "Dispose");
        }

        public void ShouldAllowCreatingInstancePerCaseUsingCustomFactory()
        {
            convention.ClassExecution
                      .CreateInstancePerCase(Factory);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Factory", ".ctor", "Pass", "Dispose",
                "Factory", ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowCreatingInstancePerTestClassUsingCustomFactory()
        {
            convention.ClassExecution
                      .CreateInstancePerTestClass(Factory);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Factory", ".ctor", "Pass", "Fail", "Dispose");
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerCaseAndConstructorThrows()
        {
            FailingMembers = new[] { ".ctor" };

            convention.ClassExecution
                      .CreateInstancePerCase();

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass failed: '.ctor' failed!",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                ".ctor");
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerTestClassAndConstructorThrows()
        {
            FailingMembers = new[] { ".ctor" };

            convention.ClassExecution
                      .CreateInstancePerTestClass();

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass failed: '.ctor' failed!",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(
                ".ctor");
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerCaseAndCustomFactoryThrows()
        {
            FailingMembers = new[] { "Factory" };

            convention.ClassExecution
                      .CreateInstancePerCase(Factory);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass failed: 'Factory' failed!",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Factory' failed!");

            output.ShouldHaveLifecycle(
                "Factory",
                "Factory");
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerTestClassAndCustomFactoryThrows()
        {
            FailingMembers = new[] { "Factory" };

            convention.ClassExecution
                      .CreateInstancePerTestClass(Factory);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass failed: 'Factory' failed!",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Factory' failed!");

            output.ShouldHaveLifecycle(
                "Factory");
        }

        public void ShouldAllowWrappingClassExecutionWithCustomBehaviors()
        {
            convention.ClassExecution
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

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Outer Before", "Inner Before",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "Inner After", "Outer After");
        }

        public void ShouldFailAllCasesWhenClassExecutionCustomBehaviorThrows()
        {
            convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap((testClass, conv, cases, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe class execution behavior");
                          throw new Exception("Unsafe class execution behavior threw!");
                      });

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass failed: Unsafe class execution behavior threw!",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: Unsafe class execution behavior threw!");

            output.ShouldHaveLifecycle("Unsafe class execution behavior");
        }

        public void ShouldAllowWrappingClassExecutionWithSetUpTearDownBehaviors()
        {
            convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "SetUp",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "TearDown");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenClassExecutionSetUpThrows()
        {
            FailingMembers = new[] { "SetUp" };

            convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass failed: 'SetUp' failed!",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'SetUp' failed!");

            output.ShouldHaveLifecycle(
                "SetUp");
        }

        public void ShouldFailAllCasesWhenClassExecutionTearDownThrows()
        {
            FailingMembers = new[] { "TearDown" };

            convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass failed: 'TearDown' failed!",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TearDown' failed!");

            output.ShouldHaveLifecycle(
                "SetUp",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "TearDown");
        }

        public void ShouldAllowWrappingInstanceExecutionWithCustomBehaviors()
        {
            convention.ClassExecution
                      .CreateInstancePerTestClass();

            convention.InstanceExecution
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

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Outer Before", "Inner Before",
                "Pass", "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenInstanceExecutionCustomBehaviorThrows()
        {
            convention.ClassExecution
                      .CreateInstancePerTestClass();

            convention.InstanceExecution
                      .Wrap((fixture, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe instance execution behavior");
                          throw new Exception("Unsafe instance execution behavior threw!");
                      });

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass failed: Unsafe instance execution behavior threw!",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: Unsafe instance execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe instance execution behavior",
                "Dispose");
        }

        public void ShouldAllowWrappingInstanceExecutionWithSetUpTearDownBehaviors()
        {
            convention.ClassExecution
                      .CreateInstancePerTestClass();

            convention.InstanceExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "Pass", "Fail",
                "TearDown",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenInstanceExecutionSetUpThrows()
        {
            FailingMembers = new[] { "SetUp" };

            convention.ClassExecution
                      .CreateInstancePerTestClass();

            convention.InstanceExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass failed: 'SetUp' failed!",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'SetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenInstanceExecutionTearDownThrows()
        {
            FailingMembers = new[] { "TearDown" };

            convention.ClassExecution
                      .CreateInstancePerTestClass();

            convention.InstanceExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Pass failed: 'TearDown' failed!",
                "Fixie.Tests.TestClasses.LifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "Pass", "Fail",
                "TearDown",
                "Dispose");
        }

        static Output Run(Convention convention)
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new StubListener();

                convention.Execute(listener, typeof(SampleTestClass));

                return new Output(console.Lines.ToArray(), listener.Entries.ToArray());
            }
        }

        class Output
        {
            readonly string[] lifecycle;
            readonly string[] results;

            public Output(string[] lifecycleLog, string[] results)
            {
                this.lifecycle = lifecycleLog;
                this.results = results;
            }

            public void ShouldHaveLifecycle(params string[] expected)
            {
                lifecycle.ShouldEqual(expected);
            }

            public void ShouldHaveResults(params string[] expected)
            {
                results.ShouldEqual(expected);
            }
        }

        class SampleTestClass : IDisposable
        {
            public SampleTestClass() { WhereAmI(); }
            public void Pass() { WhereAmI(); }
            public void Fail() { WhereAmI(); throw new FailureException(); }
            public void Dispose() { WhereAmI(); }
        }

        static object Factory(Type testClass)
        {
            WhereAmI();
            testClass.ShouldEqual(typeof(SampleTestClass));
            return new SampleTestClass();
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

        static void WhereAmI([CallerMemberName] string member = null)
        {
            Console.WriteLine(member);

            if (FailingMembers != null && FailingMembers.Contains(member))
                throw new FailureException(member);
        }
    }
}