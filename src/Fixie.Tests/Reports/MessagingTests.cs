namespace Fixie.Tests.Reports
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Reports;
    using static Utility;

    public abstract class MessagingTests
    {
        protected MessagingTests()
        {
            TestClass = FullName<SampleTestClass>();
            GenericTestClass = FullName<SampleGenericTestClass>();
        }

        protected string TestClass { get; }
        protected string GenericTestClass { get; }
        protected static Type TestClassType => typeof(SampleTestClass);

        readonly Type[] candidateTypes =
        {
            typeof(SampleTestClass),
            typeof(SampleGenericTestClass),
            typeof(EmptyTestClass)
        };

        protected class Output
        {
            public Output(string[] console)
                => Console = console;

            public string[] Console { get; }
        }

        protected async Task DiscoverAsync(Report report)
        {
            var discovery = new SelfTestDiscovery();

            using var console = new RedirectedConsole();

            await Utility.DiscoverAsync(report, discovery, candidateTypes);

            console.Lines().ShouldBeEmpty();
        }

        protected Task<Output> RunAsync(Report report)
        {
            return RunAsync(report, new SelfTestDiscovery());
        }

        protected async Task<Output> RunAsync(Report report, Discovery discovery)
        {
            var execution = new MessagingTestsExecution();

            using var console = new RedirectedConsole();

            await Utility.RunAsync(report, discovery, execution, candidateTypes);

            return new Output(console.Lines().ToArray());
        }

        class MessagingTestsExecution : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var test in testAssembly.Tests)
                {
                    if (test.Has<SkipAttribute>(out var skip))
                    {
                        await test.SkipAsync(skip.Reason);
                        continue;
                    }

                    await test.RunAsync(UsingInputAttributes);
                }
            }
        }

        protected class Base
        {
            public void Pass()
            {
                WhereAmI();
            }

            protected static void WhereAmI([CallerMemberName] string member = default!)
            {
                System.Console.Out.WriteLine("Console.Out: " + member);
                System.Console.Error.WriteLine("Console.Error: " + member);
            }
        }

        class SampleTestClass : Base
        {
            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void FailByAssertion()
            {
                WhereAmI();
                1.ShouldBe(2);
            }

            [Skip]
            public void SkipWithoutReason()
            {
                throw new ShouldBeUnreachableException();
            }

            const string alert = "\x26A0";
            [Skip(alert + " Skipped with reason.")]
            public void SkipWithReason()
            {
                throw new ShouldBeUnreachableException();
            }
        }

        protected class SampleGenericTestClass
        {
            [Input("abc")]
            [Input(123)]
            public void ShouldBeString<T>(T genericArgument)
            {
                genericArgument.ShouldBe<string>();
            }
        }

        class EmptyTestClass
        {
        }

        protected static string At(string method)
            => At<SampleTestClass>(method);

        protected static string At<T>(string method)
            => Utility.At<T>(method);

        protected static string TestClassPath()
            => PathToThisFile();
    }
}