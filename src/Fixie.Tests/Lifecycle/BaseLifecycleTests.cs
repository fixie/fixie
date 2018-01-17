namespace Fixie.Tests.Lifecycle
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Fixie.Execution;

    public abstract class BaseLifecycleTests
    {
        static string[] FailingMembers;
        protected readonly Convention Convention;

        protected BaseLifecycleTests()
        {
            FailingMembers = null;

            Convention = new Convention();
            Convention.Classes.Where(testClass => testClass == typeof(SampleTestClass));
            Convention.Methods.Where(method => method.Name == "Pass" || method.Name == "Fail");
            Convention.ClassExecution.SortCases((x, y) => String.Compare(y.Name, x.Name, StringComparison.Ordinal));
        }

        protected static void FailDuring(params string[] failingMemberNames)
        {
            FailingMembers = failingMemberNames;
        }

        protected Output Run()
        {
            var listener = new StubListener();

            using (var console = new RedirectedConsole())
            {
                Utility.Run<SampleTestClass>(listener, Convention);

                return new Output(console.Lines().ToArray(), listener.Entries.ToArray());
            }
        }

        protected class Output
        {
            readonly string[] lifecycle;
            readonly string[] results;

            public Output(string[] lifecycle, string[] results)
            {
                this.lifecycle = lifecycle;
                this.results = results;
            }

            public void ShouldHaveLifecycle(params string[] expected)
            {
                lifecycle.ShouldEqual(expected);
            }

            public void ShouldHaveResults(params string[] expected)
            {
                var namespaceQualifiedExpectation = expected.Select(x => "Fixie.Tests.Lifecycle.BaseLifecycleTests+" + x).ToArray();

                results.ShouldEqual(namespaceQualifiedExpectation);
            }
        }

        protected class SampleTestClass : IDisposable
        {
            bool disposed;

            public SampleTestClass()
            {
                WhereAmI();
            }

            public void SetUpA()
            {
                WhereAmI();
            }

            public void SetUpB()
            {
                WhereAmI();
            }

            public void Pass()
            {
                WhereAmI();
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void TearDownA()
            {
                WhereAmI();
            }

            public void TearDownB()
            {
                WhereAmI();
            }

            public void Dispose()
            {
                if (disposed)
                    throw new ShouldBeUnreachableException();
                disposed = true;

                WhereAmI();
            }
        }

        protected static void WhereAmI([CallerMemberName] string member = null)
        {
            Console.WriteLine(member);

            if (FailingMembers != null && FailingMembers.Contains(member))
                throw new FailureException(member);
        }
    }
}