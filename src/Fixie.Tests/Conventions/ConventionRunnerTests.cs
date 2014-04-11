using System;
using Fixie.Conventions;

namespace Fixie.Tests.Conventions
{
    public class ConventionRunnerTests
    {
        public void ShouldExecuteAllCasesInAllDiscoveredTestClasses()
        {
            var listener = new StubListener();
            var convention = new SelfTestConvention();

            var conventionRunner = new ConventionRunner();
            conventionRunner.Run(convention, listener, typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int), typeof(PassFailTestClass), typeof(SkipTestClass));

            listener.Entries.ShouldEqual("Fixie.Tests.Conventions.ConventionRunnerTests+PassTestClass.PassA passed.",
                "Fixie.Tests.Conventions.ConventionRunnerTests+PassTestClass.PassB passed.",
                "Fixie.Tests.Conventions.ConventionRunnerTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Conventions.ConventionRunnerTests+PassFailTestClass.Pass passed.",
                "Fixie.Tests.Conventions.ConventionRunnerTests+SkipTestClass.Skip skipped.");
        }

        public void ShouldAllowRandomShufflingOfCaseExecutionOrder()
        {
            var listener = new StubListener();
            var convention = new SelfTestConvention();

            convention.ClassExecution
                .CreateInstancePerClass()
                .ShuffleCases(new Random(1));

            var conventionRunner = new ConventionRunner();
            conventionRunner.Run(convention, listener, typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int), typeof(PassFailTestClass), typeof(SkipTestClass));

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