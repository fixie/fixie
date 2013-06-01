using System;
using System.IO;
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

            convention.FixtureExecutionBehavior.ShouldBeType<CreateInstancePerCase>();

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
            var convention = new SelfTestConvention { FixtureExecutionBehavior = new CreateInstancePerFixture() };

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

        public void ShouldSupportReplacingTheCaseExecutionBehavior()
        {
            var convention = new SelfTestConvention
            {
                CaseExecutionBehavior = new SkipCase()
            };

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
            convention.CaseExecutionBehavior = new BeforeAfterMethod("BeforeCase", convention.CaseExecutionBehavior, "AfterCase");

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
            var convention = new SelfTestConvention
            {
                InstanceExecutionBehavior = new SkipCases()
            };

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
            convention.InstanceExecutionBehavior = new BeforeAfterInstance("BeforeInstance", convention.InstanceExecutionBehavior, "AfterInstance");

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
            var convention = new SelfTestConvention
            {
                FixtureExecutionBehavior = new SkipFixture()
            };

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Skipping FirstFixture")
                    .AppendLine("Skipping SecondFixture")
                    .ToString());
        }

        public void ShouldSupportAugmentingTheFixtureExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.FixtureExecutionBehavior = new BeforeAfterType("BeforeFixture", convention.FixtureExecutionBehavior, "AfterFixture");

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
            convention.FixtureExecutionBehavior = new BeforeAfterType("BeforeFixture", convention.FixtureExecutionBehavior, "AfterFixture");
            convention.InstanceExecutionBehavior = new BeforeAfterInstance("BeforeInstance", convention.InstanceExecutionBehavior, "AfterInstance");
            convention.CaseExecutionBehavior = new BeforeAfterMethod("BeforeCase", convention.CaseExecutionBehavior, "AfterCase");

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
                throw new Exception();
            }

            public void Dispose()
            {
                indent--;
                WriteLine("Dispose {0}", GetType().Name);
            }
        }

        private static void WriteLine(string format, params object[] args)
        {
            var indentation = string.Concat(Enumerable.Repeat("    ", indent));
            Console.WriteLine(indentation + format, args);
        }

        class FirstFixture : FixturBase { }
        class SecondFixture : FixturBase { }

        class SkipCase : MethodBehavior
        {
            public void Execute(MethodInfo method, object instance, ExceptionList exceptions)
            {
                WriteLine("Skipping " + method.Name);
            }
        }

        class SkipCases : InstanceBehavior
        {
            public void Execute(Type fixtureClass, object instance, Case[] cases, Convention convention)
            {
                WriteLine("Skipping {0} case(s) for an instance of {1}", cases.Length, fixtureClass.Name);
            }
        }

        class SkipFixture : TypeBehavior
        {
            public void Execute(Type fixtureClass, Convention convention, Case[] cases)
            {
                WriteLine("Skipping " + fixtureClass.Name);
            }
        }

        class BeforeAfterMethod : MethodBehavior
        {
            readonly string before;
            readonly MethodBehavior inner;
            readonly string after;

            public BeforeAfterMethod(string before, MethodBehavior inner, string after)
            {
                this.before = before;
                this.inner = inner;
                this.after = after;
            }

            public void Execute(MethodInfo method, object fixtureInstance, ExceptionList exceptions)
            {
                WriteLine(before);
                indent++;
                inner.Execute(method, fixtureInstance, exceptions);
                indent--;
                WriteLine(after);
            }
        }

        class BeforeAfterInstance : InstanceBehavior
        {
            readonly string before;
            readonly InstanceBehavior inner;
            readonly string after;

            public BeforeAfterInstance(string before, InstanceBehavior inner, string after)
            {
                this.before = before;
                this.inner = inner;
                this.after = after;
            }

            public void Execute(Type fixtureClass, object instance, Case[] cases, Convention convention)
            {
                WriteLine(before);
                indent++;
                inner.Execute(fixtureClass, instance, cases, convention);
                indent--;
                WriteLine(after);
            }
        }

        class BeforeAfterType : TypeBehavior
        {
            readonly string before;
            readonly TypeBehavior inner;
            readonly string after;

            public BeforeAfterType(string before, TypeBehavior inner, string after)
            {
                this.before = before;
                this.inner = inner;
                this.after = after;
            }

            public void Execute(Type fixtureClass, Convention convention, Case[] cases)
            {
                WriteLine(before);
                indent++;
                inner.Execute(fixtureClass, convention, cases);
                indent--;
                WriteLine(after);
            }
        }
    }
}