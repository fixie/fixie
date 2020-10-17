namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;
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

        protected void Discover(Listener listener, out IEnumerable<string> consoleLines)
        {
            var discovery = new SelfTestDiscovery();

            using var console = new RedirectedConsole();

            Utility.Discover(listener, discovery, candidateTypes);

            consoleLines = console.Lines();
        }

        protected Output Run(Listener listener)
        {
            return Run(listener, new SelfTestDiscovery());
        }

        protected Output Run(Listener listener, Discovery discovery)
        {
            var execution = new MessagingTestsExecution();

            using var console = new RedirectedConsole();

            Utility.Run(listener, discovery, execution, candidateTypes).GetAwaiter().GetResult();

            return new Output(console.Lines().ToArray());
        }

        class MessagingTestsExecution : Execution
        {
            public async Task Execute(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                {
                    if (test.Method.Has<SkipAttribute>(out var skip))
                    {
                        test.Skip(skip.Reason);
                        continue;
                    }

                    await test.RunCases(UsingInputAttributes);
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