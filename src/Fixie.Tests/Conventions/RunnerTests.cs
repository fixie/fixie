using System;

namespace Fixie.Tests.Conventions
{
    public class ConventionRunnerTests
    {
        public void ShouldExecuteAllCasesInAllDiscoveredTestClasses()
        {
            var listener = new StubListener();
            var runner = new Runner(listener);
            var convention = SelfTestConvention.Build();

            runner.RunTypes(GetType().Assembly, convention,
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass));

            listener.Entries.ShouldEqual("Fixie.Tests.Conventions.ConventionRunnerTests+PassTestClass.PassA passed.",
                "Fixie.Tests.Conventions.ConventionRunnerTests+PassTestClass.PassB passed.",
                "Fixie.Tests.Conventions.ConventionRunnerTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Conventions.ConventionRunnerTests+PassFailTestClass.Pass passed.",
                "Fixie.Tests.Conventions.ConventionRunnerTests+SkipTestClass.Skip skipped.");
        }

        public void ShouldAllowRandomShufflingOfCaseExecutionOrder()
        {
            var listener = new StubListener();
            var runner = new Runner(listener);
            var convention = SelfTestConvention.Build();

            convention.ClassExecution
                .CreateInstancePerClass()
                .ShuffleCases(new Random(1));

            runner.RunTypes(GetType().Assembly, convention,
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass));

            listener.Entries.ShouldEqual("Fixie.Tests.Conventions.ConventionRunnerTests+PassTestClass.PassB passed.",
                "Fixie.Tests.Conventions.ConventionRunnerTests+PassTestClass.PassA passed.",
                "Fixie.Tests.Conventions.ConventionRunnerTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Conventions.ConventionRunnerTests+PassFailTestClass.Pass passed.",
                "Fixie.Tests.Conventions.ConventionRunnerTests+SkipTestClass.Skip skipped.");
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