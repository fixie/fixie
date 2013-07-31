using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.TestClasses
{
    public class ClassExecutionLifecycleTests
    {
        static string[] FailingMembers;
        readonly Convention convention;

        public ClassExecutionLifecycleTests()
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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail", "Dispose");
        }

        public void ShouldAllowCreatingInstancePerCaseUsingCustomFactory()
        {
            convention.ClassExecution
                      .CreateInstancePerCase(Factory);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass failed: '.ctor' failed!",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: '.ctor' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass failed: '.ctor' failed!",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: '.ctor' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass failed: 'Factory' failed!",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'Factory' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass failed: 'Factory' failed!",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'Factory' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass failed: Unsafe class execution behavior threw!",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: Unsafe class execution behavior threw!");

            output.ShouldHaveLifecycle("Unsafe class execution behavior");
        }

        public void ShouldAllowWrappingClassExecutionWithSetUpTearDownBehaviors()
        {
            convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run(convention);

            output.ShouldHaveResults(
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass passed.",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass failed: 'SetUp' failed!",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'SetUp' failed!");

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
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Pass failed: 'TearDown' failed!",
                "Fixie.Tests.TestClasses.ClassExecutionLifecycleTests+SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TearDown' failed!");

            output.ShouldHaveLifecycle(
                "SetUp",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "TearDown");
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

        static void WhereAmI([CallerMemberName] string member = null)
        {
            Console.WriteLine(member);

            if (FailingMembers != null && FailingMembers.Contains(member))
                throw new FailureException(member);
        }
    }
}