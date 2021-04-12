namespace Fixie.Tests.Internal
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;
    using static Utility;

    public class RunnerTests
    {
        static readonly string Self = FullName<RunnerTests>();

        public async Task ShouldPerformDiscoveryPhase()
        {
            var report = new StubReport();

            var candidateTypes = new[]
            {
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass)
            };
            var discovery = new SelfTestDiscovery();
            
            var environment = new TestEnvironment(GetType().Assembly, Console.Out, Directory.GetCurrentDirectory());
            var runner = new Runner(environment, report);
            await runner.DiscoverAsync(candidateTypes, discovery);

            report.Entries.ShouldBe(
                Self + "+PassTestClass.PassA discovered",
                Self + "+PassTestClass.PassB discovered",
                Self + "+PassFailTestClass.Fail discovered",
                Self + "+PassFailTestClass.Pass discovered",
                Self + "+SkipTestClass.SkipA discovered",
                Self + "+SkipTestClass.SkipB discovered");
        }

        public async Task ShouldPerformExecutionPhase()
        {
            var report = new StubReport();

            var candidateTypes = new[]
            {
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass)
            };
            var discovery = new SelfTestDiscovery();
            var execution = new CreateInstancePerCase();

            var environment = new TestEnvironment(GetType().Assembly, Console.Out, Directory.GetCurrentDirectory());
            var runner = new Runner(environment, report);
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
                foreach (var test in testAssembly.Tests)
                    if (!test.Name.Contains("Skip"))
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