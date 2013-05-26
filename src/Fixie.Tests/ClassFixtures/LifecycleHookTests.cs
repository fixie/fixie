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

        public void ShouldPerformFullFixtureLifecyclePerCaseMethodByDefault()
        {
            var convention = new SelfTestConvention();

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

        public void ShouldSupportAugmentingBothTheFixtureAndTheCaseExecutionBehaviors()
        {
            var convention = new SelfTestConvention();
            convention.FixtureExecutionBehavior = new BeforeAfterType("BeforeFixture", convention.FixtureExecutionBehavior, "AfterFixture");
            convention.CaseExecutionBehavior = new BeforeAfterMethod("BeforeCase", convention.CaseExecutionBehavior, "AfterCase");

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("BeforeFixture")
                    .AppendLine("    Construct FirstFixture")
                    .AppendLine("        BeforeCase")
                    .AppendLine("            PassingCase")
                    .AppendLine("        AfterCase")
                    .AppendLine("    Dispose FirstFixture")
                    .AppendLine("    Construct FirstFixture")
                    .AppendLine("        BeforeCase")
                    .AppendLine("            FailingCase Throws Exception")
                    .AppendLine("        AfterCase")
                    .AppendLine("    Dispose FirstFixture")
                    .AppendLine("AfterFixture")
                    .AppendLine("BeforeFixture")
                    .AppendLine("    Construct SecondFixture")
                    .AppendLine("        BeforeCase")
                    .AppendLine("            PassingCase")
                    .AppendLine("        AfterCase")
                    .AppendLine("    Dispose SecondFixture")
                    .AppendLine("    Construct SecondFixture")
                    .AppendLine("        BeforeCase")
                    .AppendLine("            FailingCase Throws Exception")
                    .AppendLine("        AfterCase")
                    .AppendLine("    Dispose SecondFixture")
                    .AppendLine("AfterFixture")
                    .ToString());
        }

        static string OutputFromSampleFixture(Convention convention)
        {
            using (var log = new StringWriter())
            using (new RedirectedConsole(log))
            {
                var listener = new StubListener();

                convention.Execute(listener, typeof(FirstFixture), typeof(SecondFixture));

                return log.ToString();
            }
        }

        class SecondFixturease : IDisposable
        {
            protected SecondFixturease()
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

        class FirstFixture : SecondFixturease { }
        class SecondFixture : SecondFixturease { }

        class SkipCase : MethodBehavior
        {
            public void Execute(MethodInfo method, object instance, ExceptionList exceptions)
            {
                WriteLine("Skipping " + method.Name);
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

        class RedirectedConsole : IDisposable
        {
            readonly TextWriter before;

            public RedirectedConsole(TextWriter writer)
            {
                before = Console.Out;
                Console.SetOut(writer);
            }

            public void Dispose()
            {
                Console.SetOut(before);
            }
        }
    }
}