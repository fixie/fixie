using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Fixie.Behaviors;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.ClassFixtures
{
    public class LifecycleHookTests
    {
        static int indent;

        public LifecycleHookTests()
        {
            indent = 0;
        }

        public void ShouldPerformFullFixtureLifecyclePerCaseByDefault()
        {
            var convention = new SelfTestConvention();

            convention.FixtureExecution.Behavior.ShouldBeType<CreateInstancePerCase>();

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    PassingCase")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    FailingCase Throws Exception")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    PassingCase")
                    .AppendLine("Dispose SecondFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    FailingCase Throws Exception")
                    .AppendLine("Dispose SecondFixture")
                    .ToString());
        }

        public void ShouldSupportOptionToPerformSingleFixtureLifecycleAcrossAllContainedCases()
        {
            var convention = new SelfTestConvention();
            convention.FixtureExecution.CreateInstancePerFixture();

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    PassingCase")
                    .AppendLine("    FailingCase Throws Exception")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    PassingCase")
                    .AppendLine("    FailingCase Throws Exception")
                    .AppendLine("Dispose SecondFixture")
                    .ToString());
        }

        public void ShouldSupportSuppressingTheCaseExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.CaseExecution.Wrap(SkipCase);

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    Skipping PassingCase")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    Skipping FailingCase")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    Skipping PassingCase")
                    .AppendLine("Dispose SecondFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    Skipping FailingCase")
                    .AppendLine("Dispose SecondFixture")
                    .ToString());
        }

        public void ShouldSupportAugmentingTheCaseExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.CaseExecution.Wrap(BeforeAfterCase);

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    BeforeCase")
                    .AppendLine("        PassingCase")
                    .AppendLine("    AfterCase")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    BeforeCase")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    AfterCase")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    BeforeCase")
                    .AppendLine("        PassingCase")
                    .AppendLine("    AfterCase")
                    .AppendLine("Dispose SecondFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    BeforeCase")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    AfterCase")
                    .AppendLine("Dispose SecondFixture")
                    .ToString());
        }

        public void ShouldSupportReplacingTheInstanceExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.InstanceExecution.Wrap(SkipInstance);

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    Skipping 1 case(s) for an instance of FirstFixture")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    Skipping 1 case(s) for an instance of FirstFixture")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    Skipping 1 case(s) for an instance of SecondFixture")
                    .AppendLine("Dispose SecondFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    Skipping 1 case(s) for an instance of SecondFixture")
                    .AppendLine("Dispose SecondFixture")
                    .ToString());
        }

        public void ShouldSupportAugmentingTheInstanceExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.InstanceExecution.Wrap(BeforeAfterInstance);

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    BeforeInstance")
                    .AppendLine("        PassingCase")
                    .AppendLine("    AfterInstance")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct FirstFixture")
                    .AppendLine("    BeforeInstance")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    AfterInstance")
                    .AppendLine("Dispose FirstFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    BeforeInstance")
                    .AppendLine("        PassingCase")
                    .AppendLine("    AfterInstance")
                    .AppendLine("Dispose SecondFixture")
                    .AppendLine("Construct SecondFixture")
                    .AppendLine("    BeforeInstance")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    AfterInstance")
                    .AppendLine("Dispose SecondFixture")
                    .ToString());
        }

        public void ShouldSupportReplacingTheFixtureExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.FixtureExecution.Wrap(SkipType);

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Skipping FirstFixture")
                    .AppendLine("Skipping SecondFixture")
                    .ToString());
        }

        public void ShouldSupportAugmentingTheFixtureExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.FixtureExecution.Wrap(BeforeAfterType);

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("BeforeFixture")
                    .AppendLine("    Construct FirstFixture")
                    .AppendLine("        PassingCase")
                    .AppendLine("    Dispose FirstFixture")
                    .AppendLine("    Construct FirstFixture")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    Dispose FirstFixture")
                    .AppendLine("AfterFixture")
                    .AppendLine("BeforeFixture")
                    .AppendLine("    Construct SecondFixture")
                    .AppendLine("        PassingCase")
                    .AppendLine("    Dispose SecondFixture")
                    .AppendLine("    Construct SecondFixture")
                    .AppendLine("        FailingCase Throws Exception")
                    .AppendLine("    Dispose SecondFixture")
                    .AppendLine("AfterFixture")
                    .ToString());
        }

        public void ShouldSupportAugmentingFixtureAndInstanceAndCaseBehaviors()
        {
            var convention = new SelfTestConvention();
            convention.FixtureExecution.Wrap(BeforeAfterType);
            convention.InstanceExecution.Wrap(BeforeAfterInstance);
            convention.CaseExecution.Wrap(BeforeAfterCase);

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("BeforeFixture")
                    .AppendLine("    Construct FirstFixture")
                    .AppendLine("        BeforeInstance")
                    .AppendLine("            BeforeCase")
                    .AppendLine("                PassingCase")
                    .AppendLine("            AfterCase")
                    .AppendLine("        AfterInstance")
                    .AppendLine("    Dispose FirstFixture")
                    .AppendLine("    Construct FirstFixture")
                    .AppendLine("        BeforeInstance")
                    .AppendLine("            BeforeCase")
                    .AppendLine("                FailingCase Throws Exception")
                    .AppendLine("            AfterCase")
                    .AppendLine("        AfterInstance")
                    .AppendLine("    Dispose FirstFixture")
                    .AppendLine("AfterFixture")
                    .AppendLine("BeforeFixture")
                    .AppendLine("    Construct SecondFixture")
                    .AppendLine("        BeforeInstance")
                    .AppendLine("            BeforeCase")
                    .AppendLine("                PassingCase")
                    .AppendLine("            AfterCase")
                    .AppendLine("        AfterInstance")
                    .AppendLine("    Dispose SecondFixture")
                    .AppendLine("    Construct SecondFixture")
                    .AppendLine("        BeforeInstance")
                    .AppendLine("            BeforeCase")
                    .AppendLine("                FailingCase Throws Exception")
                    .AppendLine("            AfterCase")
                    .AppendLine("        AfterInstance")
                    .AppendLine("    Dispose SecondFixture")
                    .AppendLine("AfterFixture")
                    .ToString());
        }

        static string OutputFromSampleFixture(Convention convention)
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new StubListener();

                convention.Execute(listener, typeof(FirstFixture), typeof(SecondFixture));

                return console.ToString();
            }
        }

        class FixturBase : IDisposable
        {
            protected FixturBase()
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

        static void SkipCase(MethodInfo method, object instance, ExceptionList exceptions, MethodBehavior inner)
        {
            WriteLine("Skipping " + method.Name);
        }

        static void BeforeAfterCase(MethodInfo method, object instance, ExceptionList exceptions, MethodBehavior inner)
        {
            WriteLine("BeforeCase");
            indent++;
            inner.Execute(method, instance, exceptions);
            indent--;
            WriteLine("AfterCase");
        }

        static void SkipInstance(Fixture fixture, InstanceBehavior inner)
        {
            WriteLine("Skipping {0} case(s) for an instance of {1}", fixture.Cases.Length, fixture.Type.Name);
        }

        static void BeforeAfterInstance(Fixture fixture, InstanceBehavior inner)
        {
            WriteLine("BeforeInstance");
            indent++;
            inner.Execute(fixture);
            indent--;
            WriteLine("AfterInstance");
        }

        static void SkipType(Type testClass, Convention convention, Case[] cases, TypeBehavior inner)
        {
            WriteLine("Skipping " + testClass.Name);
        }

        static void BeforeAfterType(Type testClass, Convention convention, Case[] cases, TypeBehavior inner)
        {
            WriteLine("BeforeFixture");
            indent++;
            inner.Execute(testClass, convention, cases);
            indent--;
            WriteLine("AfterFixture");
        }

        class FirstFixture : FixturBase { }
        class SecondFixture : FixturBase { }
    }
}