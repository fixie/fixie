namespace Fixie.Tests.Internal
{
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;
    using static Utility;

    public class RunnerTests
    {
        static readonly string Self = FullName<RunnerTests>();

        public async Task ShouldDiscoverAllTestsInAllDiscoveredTestClasses()
        {
            var report = new StubReport();

            var candidateTypes = new[]
            {
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass)
            };
            var discovery = new SelfTestDiscovery();
            
            var runner = new Runner(GetType().Assembly, report);
            await runner.DiscoverAsync(candidateTypes, discovery);

            report.Entries.ShouldBe(
                Self + "+PassTestClass.PassA discovered",
                Self + "+PassTestClass.PassB discovered",
                Self + "+PassFailTestClass.Fail discovered",
                Self + "+PassFailTestClass.Pass discovered",
                Self + "+SkipTestClass.SkipA discovered",
                Self + "+SkipTestClass.SkipB discovered");
        }

        public async Task ShouldExecuteAllCasesInAllDiscoveredTestClasses()
        {
            var report = new StubReport();

            var candidateTypes = new[]
            {
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass)
            };
            var discovery = new SelfTestDiscovery();
            var execution = new CreateInstancePerCase();

            var runner = new Runner(GetType().Assembly, report);
            await runner.RunAsync(candidateTypes, discovery, execution, ImmutableHashSet<string>.Empty);

            report.Entries.ShouldBe(
                Self + "+PassTestClass.PassA passed",
                Self + "+PassTestClass.PassB passed",
                Self + "+PassFailTestClass.Fail failed: 'Fail' failed!",
                Self + "+PassFailTestClass.Pass passed",
                Self + "+SkipTestClass.SkipA skipped: This test did not run.",
                Self + "+SkipTestClass.SkipB skipped: This test did not run.");
        }

        class CreateInstancePerCase : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var testClass in testAssembly.TestClasses)
                    foreach (var test in testClass.Tests)
                        if (!test.Method.Name.Contains("Skip"))
                            await test.RunAsync();
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