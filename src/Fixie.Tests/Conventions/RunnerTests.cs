using System;

namespace Fixie.Tests.Conventions
{
    public class RunnerTests
    {
        public void ShouldExecuteAllCasesInAllDiscoveredTestClasses()
        {
            var listener = new StubListener();
            var convention = SelfTestConvention.Build();

            new Runner(listener).RunTypes(GetType().Assembly, convention,
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass));

            listener.Entries.ShouldEqual("Fixie.Tests.Conventions.RunnerTests+PassTestClass.PassA passed.",
                "Fixie.Tests.Conventions.RunnerTests+PassTestClass.PassB passed.",
                "Fixie.Tests.Conventions.RunnerTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Conventions.RunnerTests+PassFailTestClass.Pass passed.",
                "Fixie.Tests.Conventions.RunnerTests+SkipTestClass.Skip skipped.");
        }

        public void ShouldAllowRandomShufflingOfCaseExecutionOrder()
        {
            var listener = new StubListener();
            var convention = SelfTestConvention.Build();

            convention.ClassExecution
                .CreateInstancePerClass()
                .ShuffleCases(new Random(1));

            new Runner(listener).RunTypes(GetType().Assembly, convention,
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass));

            listener.Entries.ShouldEqual("Fixie.Tests.Conventions.RunnerTests+PassTestClass.PassB passed.",
                "Fixie.Tests.Conventions.RunnerTests+PassTestClass.PassA passed.",
                "Fixie.Tests.Conventions.RunnerTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Conventions.RunnerTests+PassFailTestClass.Pass passed.",
                "Fixie.Tests.Conventions.RunnerTests+SkipTestClass.Skip skipped.");
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
            public void Pass() { }
            public void Fail() { throw new FailureException(); }
        }

        class SkipTestClass
        {
            public void Skip() { throw new ShouldBeUnreachableException(); }
        }
    }
}