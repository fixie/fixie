namespace Fixie.Tests
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Execution;
    using static Utility;

    public abstract class MessagingTests
    {
        protected MessagingTests()
        {
            TestClass = FullName<SampleTestClass>();
        }

        protected string TestClass { get; }

        protected void Run(Listener listener, Action<Convention> customize = null)
        {
            var convention = new Convention();

            convention
                .Classes
                .Where(testClass => testClass == typeof(SampleTestClass) || testClass == typeof(EmptyTestClass));

            convention
                .Methods
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            convention
                .CaseExecution
                .Skip(
                    x => x.Has<SkipAttribute>(),
                    x => x.GetCustomAttribute<SkipAttribute>().Reason);

            customize?.Invoke(convention);

            RunTypes(listener, convention, typeof(SampleTestClass), typeof(EmptyTestClass));
        }

        protected class Base
        {
            public void Pass()
            {
                WhereAmI();
            }

            protected static void WhereAmI([CallerMemberName] string member = null)
            {
                Console.Out.WriteLine("Console.Out: " + member);
                Console.Error.WriteLine("Console.Error: " + member);
            }
        }

        protected class SampleTestClass : Base
        {
            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void FailByAssertion()
            {
                WhereAmI();
                1.ShouldEqual(2);
            }

            [Skip]
            public void SkipWithoutReason()
            {
                throw new ShouldBeUnreachableException();
            }

            [Skip("Skipped with reason.")]
            public void SkipWithReason()
            {
                throw new ShouldBeUnreachableException();
            }
        }

        protected class EmptyTestClass
        {
        }

        protected static string At(string method)
            => At<SampleTestClass>(method);

        protected static string TestClassPath()
            => PathToThisFile();

        static string PathToThisFile([CallerFilePath] string path = null)
            => path;
    }
}