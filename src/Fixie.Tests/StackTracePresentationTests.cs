namespace Fixie.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Internal.Listeners;
    using static Utility;

    public class StackTracePresentationTests
    {
        public void ShouldProvideCleanStackTraceForImplicitTestClassConstructionFailures()
        {
            Run<ConstructionFailureTestClass, ImplicitConstruction>()
                .ShouldBe(
                    "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' failed:",
                    "",
                    "'.ctor' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<ConstructionFailureTestClass>(".ctor()"),
                    "",
                    "1 failed, took 1.23 seconds");
        }
        
        public void ShouldProvideCleanStackTraceForExplicitTestClassConstructionFailures()
        {
            Run<ConstructionFailureTestClass, ExplicitConstruction>()
                .ShouldBe(
                    "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' skipped:",
                    "This test did not run.",
                    "",
                    "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' failed:",
                    "",
                    "'.ctor' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<ConstructionFailureTestClass>(".ctor()"),
                    "",
                    "1 failed, 1 skipped, took 1.23 seconds");
        }

        public void ShouldProvideCleanStackTraceTestMethodFailures()
        {
            Run<FailureTestClass, ImplicitConstruction>()
                .ShouldBe(
                    "Test '" + FullName<FailureTestClass>() + ".Asynchronous' failed:",
                    "",
                    "'Asynchronous' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<FailureTestClass>("Asynchronous()"),
                    "",
                    "Test '" + FullName<FailureTestClass>() + ".Synchronous' failed:",
                    "",
                    "'Synchronous' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<FailureTestClass>("Synchronous()"),
                    "",
                    "2 failed, took 1.23 seconds");
        }

        static IEnumerable<string> Run<TSampleTestClass, TExecution>() where TExecution : Execution, new()
        {
            var listener = new ConsoleListener();
            var discovery = new SelfTestDiscovery();
            var execution = new TExecution();
            
            using var console = new RedirectedConsole();

            Utility.Run(listener, discovery, execution, typeof(TSampleTestClass));

            return console.Lines()
                .CleanStackTraceLineNumbers()
                .CleanDuration();
        }

        class ImplicitConstruction : Execution
        {
            public void Execute(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                    test.Run();
            }
        }

        class ExplicitConstruction : Execution
        {
            public void Execute(TestClass testClass)
            {
                var instance = testClass.Construct();
                foreach (var test in testClass.Tests)
                    test.Run(instance);
            }
        }

        class ConstructionFailureTestClass
        {
            public ConstructionFailureTestClass() => throw new FailureException();
            public void UnreachableTest() => throw new ShouldBeUnreachableException();
        }

        class FailureTestClass
        {
            public void Synchronous()
            {
                throw new FailureException();
            }

            public async Task Asynchronous()
            {
                await Task.Yield();
                throw new FailureException();
            }
        }
    }
}