using System;
using System.IO;
using System.Reflection;
using System.Text;
using Should;

namespace Fixie.Tests.ClassFixtures
{
    public class LifecycleTests
    {
        public void ShouldInvokeCaseMethodsByDefault()
        {
            var convention = new SelfTestConvention();

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct")
                    .AppendLine("PassingCase")
                    .AppendLine("Dispose")
                    .AppendLine("Construct")
                    .AppendLine("FailingCase Throws Exception")
                    .AppendLine("Dispose")
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
                    .AppendLine("Construct")
                    .AppendLine("Skipping PassingCase")
                    .AppendLine("Dispose")
                    .AppendLine("Construct")
                    .AppendLine("Skipping FailingCase")
                    .AppendLine("Dispose")
                    .ToString());
        }

        public void ShouldSupportAugmentingTheCaseExecutionBehavior()
        {
            var convention = new SelfTestConvention();
            convention.CaseExecutionBehavior = new BeforeAfter("SetUp", convention.CaseExecutionBehavior, "TearDown");

            OutputFromSampleFixture(convention).ShouldEqual(
                new StringBuilder()
                    .AppendLine("Construct")
                    .AppendLine("SetUp")
                    .AppendLine("PassingCase")
                    .AppendLine("TearDown")
                    .AppendLine("Dispose")
                    .AppendLine("Construct")
                    .AppendLine("SetUp")
                    .AppendLine("FailingCase Throws Exception")
                    .AppendLine("TearDown")
                    .AppendLine("Dispose")
                    .ToString());
        }

        static string OutputFromSampleFixture(Convention convention)
        {
            using (var log = new StringWriter())
            using (new RedirectedConsole(log))
            {
                var listener = new StubListener();

                convention.Execute(listener, typeof(SampleFixture));

                return log.ToString();
            }
        }

        class SampleFixture : IDisposable
        {
            public SampleFixture()
            {
                Console.WriteLine("Construct");
            }

            public void PassingCase()
            {
                Console.WriteLine("PassingCase");
            }

            public void FailingCase()
            {
                Console.WriteLine("FailingCase Throws Exception");
                throw new Exception();
            }

            public void Dispose()
            {
                Console.WriteLine("Dispose");
            }
        }

        class SkipCase : MethodBehavior
        {
            public void Execute(MethodInfo method, object instance, ExceptionList exceptions)
            {
                Console.WriteLine("Skipping " + method.Name);
            }
        }

        class BeforeAfter : MethodBehavior
        {
            readonly string before;
            readonly MethodBehavior inner;
            readonly string after;

            public BeforeAfter(string before, MethodBehavior inner, string after)
            {
                this.before = before;
                this.inner = inner;
                this.after = after;
            }

            public void Execute(MethodInfo method, object fixtureInstance, ExceptionList exceptions)
            {
                Console.WriteLine(before);
                inner.Execute(method, fixtureInstance, exceptions);
                Console.WriteLine(after);
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