namespace Fixie.Tests.Internal
{
    using System.Collections.Immutable;
    using Assertions;
    using Fixie.Internal;
    using static Utility;

    public class RunnerTests
    {
        static readonly string Self = FullName<RunnerTests>();

        public void ShouldDiscoverAllTestsInAllDiscoveredTestClasses()
        {
            var listener = new StubListener();

            var candidateTypes = new[]
            {
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass)
            };
            var discovery = new SelfTestDiscovery();

            new Runner(GetType().Assembly, listener).Discover(candidateTypes, discovery);

            listener.Entries.ShouldBe(
                Self + "+PassTestClass.PassA discovered",
                Self + "+PassTestClass.PassB discovered",
                Self + "+PassFailTestClass.Fail discovered",
                Self + "+PassFailTestClass.Pass discovered",
                Self + "+SkipTestClass.SkipA discovered",
                Self + "+SkipTestClass.SkipB discovered");
        }

        public void ShouldExecuteAllCasesInAllDiscoveredTestClasses()
        {
            var listener = new StubListener();

            var candidateTypes = new[]
            {
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass)
            };
            var discovery = new SelfTestDiscovery();
            var execution = new CreateInstancePerCase();

            new Runner(GetType().Assembly, listener).Run(candidateTypes, discovery, execution, ImmutableHashSet<string>.Empty);

            listener.Entries.ShouldBe(
                Self + "+PassTestClass.PassA passed",
                Self + "+PassTestClass.PassB passed",
                Self + "+PassFailTestClass.Fail failed: 'Fail' failed!",
                Self + "+PassFailTestClass.Pass passed",
                Self + "+SkipTestClass.SkipA skipped: This test did not run.",
                Self + "+SkipTestClass.SkipB skipped: This test did not run.");
        }

        class CreateInstancePerCase : Execution
        {
            public void Execute(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                    if (!test.Method.Name.Contains("Skip"))
                        test.Run();
            }
        }

        class SampleIrrelevantClass
        {
            public void PassA() { }
            public void PassB() { }
        }

        class PassTestClass
        {
            public void PassA() { }
            public void PassB() { }
        }

        class PassFailTestClass
        {
            public void Fail() { throw new FailureException(); }
            public void Pass() { }
        }

        class SkipTestClass
        {
            public void SkipA() { throw new ShouldBeUnreachableException(); }
            public void SkipB() { throw new ShouldBeUnreachableException(); }
        }
    }
}