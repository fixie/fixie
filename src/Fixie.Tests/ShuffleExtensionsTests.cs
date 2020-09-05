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
            Run<SampleTestClass, ShuffleExecution>()
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