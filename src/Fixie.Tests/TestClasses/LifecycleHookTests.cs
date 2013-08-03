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

        static void BeforeAfterCase(Case @case, object instance, Action innerBehavior)
        {
            WriteLine("BeforeCase");
            indent++;
            innerBehavior();
            indent--;
            WriteLine("AfterCase");
        }

        static void BeforeAfterInstance(Fixture fixture, Action innerBehavior)
        {
            WriteLine("BeforeInstance");
            indent++;
            innerBehavior();
            indent--;
            WriteLine("AfterInstance");
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