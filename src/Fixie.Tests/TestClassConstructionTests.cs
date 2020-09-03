namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Internal;

    public class TestClassConstructionTests
    {
        static string[] Run<TSampleTestClass, TExecution>() where TExecution : Execution, new()
            => Run<TExecution>(typeof(TSampleTestClass));

        static string[] Run<TExecution>(Type testClass) where TExecution : Execution, new()
        {
            var listener = new StubListener();
            var discovery = new SelfTestDiscovery();
            var execution = new TExecution();
            using var console = new RedirectedConsole();

            Utility.Run(listener, discovery, execution, testClass);

            return console.Lines().ToArray();
        }

        class SampleTestClass : IDisposable
        {
            bool disposed;

            public SampleTestClass()
            {
                WhereAmI();
            }

            [Input(1)]
            [Input(2)]
            public void Pass(int i)
            {
                WhereAmI(i);
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
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
            public static void Pass()
            {
                WhereAmI();
            }

            public static void Fail()
            {
                WhereAmI();
                throw new FailureException();
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
                {
                    try
                    {
                        if (!ShouldSkip(test))
                            test.RunCases(UsingInputAttibutes);
                    }
                    catch (Exception exception)
                    {
                        test.Fail(exception);
                    }
                }
            }
        }

        class CreateInstancePerClass : Execution
        {
            public void Execute(TestClass testClass)
            {
                var type = testClass.Type;
                var instance = type.IsStatic() ? null : Activator.CreateInstance(type);

                foreach (var test in testClass.Tests)
                {
                    try
                    {
                        if (!ShouldSkip(test))
                            test.RunCases(UsingInputAttibutes, instance);
                    }
                    catch (Exception exception)
                    {
                        test.Fail(exception);
                    }
                }

                instance.Dispose();
            }
        }

        static IEnumerable<object?[]> UsingInputAttibutes(MethodInfo method)
            => method
                .GetCustomAttributes<InputAttribute>(true)
                .Select(input => input.Parameters);

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