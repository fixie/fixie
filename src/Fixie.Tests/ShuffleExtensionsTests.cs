namespace Fixie.Tests
{
    using System;
    using Assertions;
    using static Utility;

    public class ShuffleExtensionsTests
    {
        const int Seed = 17;

        public void ShouldEnableRandomShufflingOfTestExecutionOrder()
        {
            var listener = new StubListener();
            var discovery = new SelfTestDiscovery();
            var execution = new ShuffleExecution();

            Run(listener, discovery, execution, typeof(SampleTestClass));

            listener.Entries
                .ShouldBe(
                    For<SampleTestClass>(
                        ".PassC passed",
                        ".PassA passed",
                        ".PassB passed",
                        ".PassE passed",
                        ".PassD passed"));
        }

        class ShuffleExecution : Execution
        {
            public void Execute(TestClass testClass)
            {
                foreach (var test in testClass.Tests.Shuffle(new Random(Seed)))
                    test.Run();
            }
        }

        class SampleTestClass
        {
            public void PassA() { }
            public void PassB() { }
            public void PassC() { }
            public void PassD() { }
            public void PassE() { }
        }
    }
}