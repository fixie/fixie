namespace Fixie.Tests
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Internal;
    using static Utility;

    public abstract class MessagingTests
    {
        protected MessagingTests()
        {
            TestClass = FullName<SampleTestClass>();
        }

        protected string TestClass { get; }
        protected Type TestClassType => typeof(SampleTestClass);

        protected void Run(Listener listener, Action<Discovery>? customize = null)
        {
            var discovery = new SelfTestDiscovery();

            customize?.Invoke(discovery);

            var execution = new CreateInstancePerCase();
            Utility.Run(listener, discovery, execution, typeof(SampleTestClass), typeof(EmptyTestClass));
        }

        class CreateInstancePerCase : Execution
        {
            public void Execute(TestClass testClass)
            {
                testClass.RunCases(@case =>
                {
                    if (@case.Method.Has<SkipAttribute>(out var skip))
                    {
                        @case.Skip(skip.Reason);
                        return;
                    }

                    var instance = testClass.Construct();

                    @case.Execute(instance);

                    instance.Dispose();
                });
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
                Console.Out.WriteLine("Console.Out: " + member);
                Console.Error.WriteLine("Console.Error: " + member);
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

        class EmptyTestClass
        {
        }

        protected static string At(string method)
            => At<SampleTestClass>(method);

        protected static string TestClassPath()
            => PathToThisFile();
    }
}