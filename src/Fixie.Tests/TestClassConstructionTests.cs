namespace Fixie.Tests
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Internal;

    public class TestClassConstructionTests
    {
        static string[] Run<TSampleTestClass, TExecution>() where TExecution : Execution, new()
            => Run<TExecution>(typeof(TSampleTestClass));

        static string[] Run<TExecution>(Type testClass) where TExecution : Execution, new()
        {
            using var console = new RedirectedConsole();

            Utility.Run<TExecution>(testClass);

            return console.Lines().ToArray();
        }

        class SampleTestClass : IDisposable
        {
            bool disposed;

            public SampleTestClass()
            {
                WhereAmI();
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            [Input(1)]
            [Input(2)]
            public void Pass(int i)
            {
                WhereAmI(i);
            }

            public void Skip()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void Dispose()
            {
                if (disposed)
                    throw new ShouldBeUnreachableException();
                disposed = true;

                WhereAmI();
            }
        }

        class AllSkippedTestClass : IDisposable
        {
            bool disposed;

            public AllSkippedTestClass()
            {
                WhereAmI();
            }

            public void SkipA()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void SkipB()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void SkipC()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void Dispose()
            {
                if (disposed)
                    throw new ShouldBeUnreachableException();
                disposed = true;

                WhereAmI();
            }
        }

        static class StaticTestClass
        {
            public static void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public static void Pass()
            {
                WhereAmI();
            }

            public static void Skip()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }
        }

        static void WhereAmI(object parameter, [CallerMemberName] string member = default!)
            => System.Console.WriteLine($"{member}({parameter})");
        
        static void WhereAmI([CallerMemberName] string member = default!)
            => System.Console.WriteLine(member);

        static bool ShouldSkip(TestMethod test)
            => test.Method.Name.Contains("Skip");

        class CreateInstancePerCase : Execution
        {
            public void Execute(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                    if (!ShouldSkip(test))
                        test.RunCases(Utility.UsingInputAttributes);
            }
        }

        class CreateInstancePerClass : Execution
        {
            public void Execute(TestClass testClass)
            {
                var type = testClass.Type;
                var instance = type.IsStatic() ? null : Activator.CreateInstance(type);

                foreach (var test in testClass.Tests)
                    if (!ShouldSkip(test))
                        test.RunCases(Utility.UsingInputAttributes, instance);

                instance.Dispose();
            }
        }

        public void ShouldConstructPerCaseByDefault()
        {
            //NOTE: With no input parameter or skip behaviors,
            //      all test methods are attempted once and with zero
            //      parameters, so Skip() is reached and Pass(int)
            //      is attempted once but never reached.

            Run<SampleTestClass, DefaultExecution>()
                .ShouldBe(
                    ".ctor", "Fail", "Dispose",
                    ".ctor", "Dispose",
                    ".ctor", "Skip", "Dispose");
        }

        public void ShouldAllowConstructingPerCase()
        {
            Run<SampleTestClass, CreateInstancePerCase>()
                .ShouldBe(
                    ".ctor", "Fail", "Dispose",
                    ".ctor", "Pass(1)", "Dispose",
                    ".ctor", "Pass(2)", "Dispose");
        }

        public void ShouldAllowConstructingPerClass()
        {
            Run<SampleTestClass, CreateInstancePerClass>()
                .ShouldBe(".ctor", "Fail", "Pass(1)", "Pass(2)", "Dispose");
        }

        public void ShouldBypassConstructionWhenConstructingPerCaseAndAllCasesAreSkipped()
        {
            Run<AllSkippedTestClass, CreateInstancePerCase>()
                .ShouldBeEmpty();
        }

        public void ShouldNotBypassConstructionWhenConstructingPerClassAndAllCasesAreSkipped()
        {
            Run<AllSkippedTestClass, CreateInstancePerClass>()
                .ShouldBe(".ctor", "Dispose");
        }

        public void ShouldBypassConstructionAttemptsWhenTestMethodsAreStatic()
        {
            Run<DefaultExecution>(typeof(StaticTestClass))
                .ShouldBe("Fail", "Pass", "Skip");

            Run<CreateInstancePerCase>(typeof(StaticTestClass))
                .ShouldBe("Fail", "Pass");

            Run<CreateInstancePerClass>(typeof(StaticTestClass))
                .ShouldBe("Fail", "Pass");
        }
    }
}