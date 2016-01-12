using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Internal;

namespace Fixie.Tests.Internal
{
    public class RunnerTests
    {
        public void ShouldExecuteAllCasesInAllDiscoveredTestClasses()
        {
            var listener = new StubListener();
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>());

            new Runner(listener).RunTypes(GetType().Assembly, convention,
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.Internal.RunnerTests+PassTestClass.PassA passed",
                "Fixie.Tests.Internal.RunnerTests+PassTestClass.PassB passed",
                "Fixie.Tests.Internal.RunnerTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Internal.RunnerTests+PassFailTestClass.Pass passed",
                "Fixie.Tests.Internal.RunnerTests+SkipTestClass.SkipA skipped",
                "Fixie.Tests.Internal.RunnerTests+SkipTestClass.SkipB skipped");
        }

        public void ShouldAllowRandomShufflingOfCaseExecutionOrder()
        {
            var listener = new StubListener();
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>());

            convention.ClassExecution
                .CreateInstancePerClass()
                .ShuffleCases(new Random(1));

            new Runner(listener).RunTypes(GetType().Assembly, convention,
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.Internal.RunnerTests+PassTestClass.PassB passed",
                "Fixie.Tests.Internal.RunnerTests+PassTestClass.PassA passed",
                "Fixie.Tests.Internal.RunnerTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Internal.RunnerTests+PassFailTestClass.Pass passed",
                "Fixie.Tests.Internal.RunnerTests+SkipTestClass.SkipB skipped",
                "Fixie.Tests.Internal.RunnerTests+SkipTestClass.SkipA skipped");
        }

        public void ShouldShortCircuitTestExecutionByFailingAllCasesWhenCaseOrderingThrows()
        {
            var listener = new StubListener();
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>());

            convention.ClassExecution
                .CreateInstancePerClass()
                .SortCases((caseA, caseB) => { throw new Exception("SortCases lambda expression threw!"); });

            convention.Parameters
                .Add<BuggyParameterSource>();

            new Runner(listener).RunTypes(GetType().Assembly, convention,
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass), typeof(BuggyParameterGenerationTestClass));

            //NOTE: Since the ordering of cases is deliberately failing, and since member order via reflection
            //      is undefined, we explicitly sort the listener Entries here to avoid making a brittle assertion.

            var strings = listener.Entries.OrderBy(x => x).ToArray();
            strings.ShouldEqual(

                "Fixie.Tests.Internal.RunnerTests+BuggyParameterGenerationTestClass.ParameterizedA failed: Exception thrown while attempting to yield input parameters for method: ParameterizedA" + Environment.NewLine +
	            "    Secondary Failure: Failed to compare two elements in the array." + Environment.NewLine +
	            "        Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Internal.RunnerTests+BuggyParameterGenerationTestClass.ParameterizedB failed: Exception thrown while attempting to yield input parameters for method: ParameterizedB" + Environment.NewLine +
	            "    Secondary Failure: Failed to compare two elements in the array." + Environment.NewLine +
	            "        Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Internal.RunnerTests+PassFailTestClass.Fail failed: Failed to compare two elements in the array." + Environment.NewLine +
                "    Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Internal.RunnerTests+PassFailTestClass.Pass failed: Failed to compare two elements in the array." + Environment.NewLine +
                "    Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Internal.RunnerTests+PassTestClass.PassA failed: Failed to compare two elements in the array." + Environment.NewLine +
                "    Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Internal.RunnerTests+PassTestClass.PassB failed: Failed to compare two elements in the array." + Environment.NewLine +
                "    Inner Exception: SortCases lambda expression threw!",

                "Fixie.Tests.Internal.RunnerTests+SkipTestClass.SkipA skipped",

                "Fixie.Tests.Internal.RunnerTests+SkipTestClass.SkipB skipped");
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
            [Skip]
            public void SkipA() { throw new ShouldBeUnreachableException(); }
            [Skip]
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

        [AttributeUsage(AttributeTargets.Method)]
        class SkipAttribute : Attribute
        {
        }
    }
}