using System;
using System.Linq;
using System.Text;
using Fixie.Behaviors;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.TestClasses
{
    public class LifecycleHookTests
    {
        static int indent;

        public LifecycleHookTests()
        {
            indent = 0;
        }

        public void ShouldPerformFullTestClassLifecyclePerCaseByDefault()
        {
            var convention = new SelfTestConvention();

            convention.ClassExecution.Behavior.ShouldBeType<CreateInstancePerCase>();

            OutputFromSampleTestClasses(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    PassingCase")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    FailingCase Throws Exception")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    PassingCase")
                    .AppendLine("Dispose SecondTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    FailingCase Throws Exception")
                    .AppendLine("Dispose SecondTestClass")
                    .ToString());
        }

        public void ShouldSupportOptionToPerformSingleTestClassLifecycleAcrossAllContainedCases()
        {
            var convention = new SelfTestConvention();
            convention.ClassExecution.CreateInstancePerTestClass();

            OutputFromSampleTestClasses(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    PassingCase")
                    .AppendLine("    FailingCase Throws Exception")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    PassingCase")
                    .AppendLine("    FailingCase Throws Exception")
                    .AppendLine("Dispose SecondTestClass")
                    .ToString());
        }

        public void ShouldSupportSuppressingTheCaseExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.CaseExecution.Wrap(SkipCase);

            OutputFromSampleTestClasses(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    Skipping PassingCase")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    Skipping FailingCase")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    Skipping PassingCase")
                    .AppendLine("Dispose SecondTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    Skipping FailingCase")
                    .AppendLine("Dispose SecondTestClass")
                    .ToString());
        }

        public void ShouldSupportAugmentingTheCaseExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.CaseExecution.Wrap(BeforeAfterCase);

            OutputFromSampleTestClasses(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    BeforeCase")
                    .AppendLine("        PassingCase")
                    .AppendLine("    AfterCase")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    BeforeCase")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    AfterCase")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    BeforeCase")
                    .AppendLine("        PassingCase")
                    .AppendLine("    AfterCase")
                    .AppendLine("Dispose SecondTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    BeforeCase")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    AfterCase")
                    .AppendLine("Dispose SecondTestClass")
                    .ToString());
        }

        public void ShouldSupportReplacingTheInstanceExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.InstanceExecution.Wrap(SkipInstance);

            OutputFromSampleTestClasses(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    Skipping 1 case(s) for an instance of FirstTestClass")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    Skipping 1 case(s) for an instance of FirstTestClass")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    Skipping 1 case(s) for an instance of SecondTestClass")
                    .AppendLine("Dispose SecondTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    Skipping 1 case(s) for an instance of SecondTestClass")
                    .AppendLine("Dispose SecondTestClass")
                    .ToString());
        }

        public void ShouldSupportAugmentingTheInstanceExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.InstanceExecution.Wrap(BeforeAfterInstance);

            OutputFromSampleTestClasses(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    BeforeInstance")
                    .AppendLine("        PassingCase")
                    .AppendLine("    AfterInstance")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct FirstTestClass")
                    .AppendLine("    BeforeInstance")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    AfterInstance")
                    .AppendLine("Dispose FirstTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    BeforeInstance")
                    .AppendLine("        PassingCase")
                    .AppendLine("    AfterInstance")
                    .AppendLine("Dispose SecondTestClass")
                    .AppendLine("Construct SecondTestClass")
                    .AppendLine("    BeforeInstance")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    AfterInstance")
                    .AppendLine("Dispose SecondTestClass")
                    .ToString());
        }

        public void ShouldSupportReplacingTheClassExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.ClassExecution.Wrap(SkipType);

            OutputFromSampleTestClasses(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Skipping FirstTestClass")
                    .AppendLine("Skipping SecondTestClass")
                    .ToString());
        }

        public void ShouldSupportAugmentingTheClassExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.ClassExecution.Wrap(BeforeAfterType);

            OutputFromSampleTestClasses(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("BeforeTestClass")
                    .AppendLine("    Construct FirstTestClass")
                    .AppendLine("        PassingCase")
                    .AppendLine("    Dispose FirstTestClass")
                    .AppendLine("    Construct FirstTestClass")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    Dispose FirstTestClass")
                    .AppendLine("AfterTestClass")
                    .AppendLine("BeforeTestClass")
                    .AppendLine("    Construct SecondTestClass")
                    .AppendLine("        PassingCase")
                    .AppendLine("    Dispose SecondTestClass")
                    .AppendLine("    Construct SecondTestClass")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    Dispose SecondTestClass")
                    .AppendLine("AfterTestClass")
                    .ToString());
        }

        public void ShouldSupportAugmentingTestClassAndInstanceAndCaseBehaviors()
        {
            var convention = new SelfTestConvention();
            convention.ClassExecution.Wrap(BeforeAfterType);
            convention.InstanceExecution.Wrap(BeforeAfterInstance);
            convention.CaseExecution.Wrap(BeforeAfterCase);

            OutputFromSampleTestClasses(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("BeforeTestClass")
                    .AppendLine("    Construct FirstTestClass")
                    .AppendLine("        BeforeInstance")
                    .AppendLine("            BeforeCase")
                    .AppendLine("                PassingCase")
                    .AppendLine("            AfterCase")
                    .AppendLine("        AfterInstance")
                    .AppendLine("    Dispose FirstTestClass")
                    .AppendLine("    Construct FirstTestClass")
                    .AppendLine("        BeforeInstance")
                    .AppendLine("            BeforeCase")
                    .AppendLine("                FailingCase Throws Exception")
                    .AppendLine("            AfterCase")
                    .AppendLine("        AfterInstance")
                    .AppendLine("    Dispose FirstTestClass")
                    .AppendLine("AfterTestClass")
                    .AppendLine("BeforeTestClass")
                    .AppendLine("    Construct SecondTestClass")
                    .AppendLine("        BeforeInstance")
                    .AppendLine("            BeforeCase")
                    .AppendLine("                PassingCase")
                    .AppendLine("            AfterCase")
                    .AppendLine("        AfterInstance")
                    .AppendLine("    Dispose SecondTestClass")
                    .AppendLine("    Construct SecondTestClass")
                    .AppendLine("        BeforeInstance")
                    .AppendLine("            BeforeCase")
                    .AppendLine("                FailingCase Throws Exception")
                    .AppendLine("            AfterCase")
                    .AppendLine("        AfterInstance")
                    .AppendLine("    Dispose SecondTestClass")
                    .AppendLine("AfterTestClass")
                    .ToString());
        }

        static string OutputFromSampleTestClasses(Convention convention)
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new StubListener();

                convention.Execute(listener, typeof(FirstTestClass), typeof(SecondTestClass));

                return console.ToString();
            }
        }

        class SampleTestClassBase : IDisposable
        {
            protected SampleTestClassBase()
            {
                WriteLine("Construct {0}", GetType().Name);
                indent++;
            }

            public void PassingCase()
            {
                WriteLine("PassingCase");
            }

            public void FailingCase()
            {
                WriteLine("FailingCase Throws Exception");
                throw new FailureException();
            }

            public void Dispose()
            {
                indent--;
                WriteLine("Dispose {0}", GetType().Name);
            }
        }

        static void WriteLine(string format, params object[] args)
        {
            var indentation = string.Concat(Enumerable.Repeat("    ", indent));
            Console.WriteLine(indentation + format, args);
        }

        static void SkipCase(Case @case, object instance, Action innerBehavior)
        {
            WriteLine("Skipping " + @case.Method.Name);
        }

        static void BeforeAfterCase(Case @case, object instance, Action innerBehavior)
        {
            WriteLine("BeforeCase");
            indent++;
            innerBehavior();
            indent--;
            WriteLine("AfterCase");
        }

        static void SkipInstance(Fixture fixture, Action innerBehavior)
        {
            WriteLine("Skipping {0} case(s) for an instance of {1}", fixture.Cases.Length, fixture.TestClass.Name);
        }

        static void BeforeAfterInstance(Fixture fixture, Action innerBehavior)
        {
            WriteLine("BeforeInstance");
            indent++;
            innerBehavior();
            indent--;
            WriteLine("AfterInstance");
        }

        static void SkipType(Type testClass, Convention convention, Case[] cases, Action innerBehavior)
        {
            WriteLine("Skipping " + testClass.Name);
        }

        static void BeforeAfterType(Type testClass, Convention convention, Case[] cases, Action innerBehavior)
        {
            WriteLine("BeforeTestClass");
            indent++;
            innerBehavior();
            indent--;
            WriteLine("AfterTestClass");
        }

        class FirstTestClass : SampleTestClassBase { }
        class SecondTestClass : SampleTestClassBase { }
    }
}