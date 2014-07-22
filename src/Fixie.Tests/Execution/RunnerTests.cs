using Fixie.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Tests.Execution
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

            listener.Entries.ShouldEqual(
                "Fixie.Tests.Execution.RunnerTests+PassTestClass.PassA passed.",
                "Fixie.Tests.Execution.RunnerTests+PassTestClass.PassB passed.",
                "Fixie.Tests.Execution.RunnerTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Execution.RunnerTests+PassFailTestClass.Pass passed.",
                "Fixie.Tests.Execution.RunnerTests+SkipTestClass.SkipA skipped.",
                "Fixie.Tests.Execution.RunnerTests+SkipTestClass.SkipB skipped.");
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

            listener.Entries.ShouldEqual(
                "Fixie.Tests.Execution.RunnerTests+PassTestClass.PassB passed.",
                "Fixie.Tests.Execution.RunnerTests+PassTestClass.PassA passed.",
                "Fixie.Tests.Execution.RunnerTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Execution.RunnerTests+PassFailTestClass.Pass passed.",
                "Fixie.Tests.Execution.RunnerTests+SkipTestClass.SkipB skipped.",
                "Fixie.Tests.Execution.RunnerTests+SkipTestClass.SkipA skipped.");
        }

        public void ShouldShortCircuitTestExecutionByFailingAllCasesWhenCaseOrderingThrows()
        {
            var listener = new StubListener();
            var convention = SelfTestConvention.Build();

            convention.ClassExecution
                .CreateInstancePerClass()
                .SortCases((caseA, caseB) => { throw new Exception("SortCases lambda expression threw!"); });

            convention.Parameters
                .Add<BuggyParameterSource>();

            convention.Traits
                      .Add<BuggyTraitSource>();

            new Runner(listener).RunTypes(GetType().Assembly, convention,
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass), typeof(BuggyParameterGenerationTestClass),
                typeof(BuggyTraitGenerationTestClass));

            //NOTE: Since the ordering of cases is deliberately failing, and since member order via reflection
            //      is undefined, we explicitly sort the listener Entries here to avoid making a brittle assertion.

            var strings = listener.Entries.OrderBy(x => x).ToArray();
            strings.ShouldEqual(

                "Fixie.Tests.Execution.RunnerTests+BuggyParameterGenerationTestClass.ParameterizedA failed: Exception thrown while attempting to yield input parameters for method: ParameterizedA" + Environment.NewLine +
                "    Secondary Failure: Failed to compare two elements in the array." + Environment.NewLine +
                "        Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Execution.RunnerTests+BuggyParameterGenerationTestClass.ParameterizedB failed: Exception thrown while attempting to yield input parameters for method: ParameterizedB" + Environment.NewLine +
                "    Secondary Failure: Failed to compare two elements in the array." + Environment.NewLine +
                "        Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Execution.RunnerTests+BuggyTraitGenerationTestClass.TraitsA failed: Exception thrown while attempting to yield traits for method: TraitsA" + Environment.NewLine +
                "    Secondary Failure: Failed to compare two elements in the array." + Environment.NewLine +
                "        Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Execution.RunnerTests+BuggyTraitGenerationTestClass.TraitsB failed: Exception thrown while attempting to yield traits for method: TraitsB" + Environment.NewLine +
                "    Secondary Failure: Failed to compare two elements in the array." + Environment.NewLine +
                "        Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Execution.RunnerTests+PassFailTestClass.Fail failed: Failed to compare two elements in the array." + Environment.NewLine +
                "    Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Execution.RunnerTests+PassFailTestClass.Pass failed: Failed to compare two elements in the array." + Environment.NewLine +
                "    Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Execution.RunnerTests+PassTestClass.PassA failed: Failed to compare two elements in the array." + Environment.NewLine +
                "    Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Execution.RunnerTests+PassTestClass.PassB failed: Failed to compare two elements in the array." + Environment.NewLine +
                "    Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Execution.RunnerTests+SkipTestClass.SkipA skipped.",

                "Fixie.Tests.Execution.RunnerTests+SkipTestClass.SkipB skipped.");
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
            public void SkipA() { throw new ShouldBeUnreachableException(); }
            public void SkipB() { throw new ShouldBeUnreachableException(); }
        }

        class BuggyParameterGenerationTestClass
        {
            public void ParameterizedA(int i) { }
            public void ParameterizedB(int i) { }
        }

        class BuggyParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                if (method.GetParameters().Length == 0)
                    yield break;

                throw new Exception("Exception thrown while attempting to yield input parameters for method: " + method.Name);
            }
        }

        class BuggyTraitGenerationTestClass
        {
            [Trait("", "")]
            public void TraitsA() { }

            [Trait("", "")]
            public void TraitsB() { }
        }

        class BuggyTraitSource : TraitSource
        {
            public IEnumerable<Trait> GetTraits(MethodInfo method)
            {
                if (!method.GetCustomAttributes<TraitAttribute>().Any())
                    yield break;

                throw new Exception("Exception thrown while attempting to yield traits for method: " + method.Name);
            }
        }
    }
}