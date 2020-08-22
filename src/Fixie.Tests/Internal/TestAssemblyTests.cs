namespace Fixie.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Assertions;
    using Fixie.Internal;
    using static Utility;

    public class TestAssemblyTests
    {
        static readonly string Self = FullName<TestAssemblyTests>();

        public void ShouldDiscoverAllTestsInAllDiscoveredTestClasses()
        {
            var listener = new StubListener();

            var candidateTypes = new[]
            {
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass)
            };
            var discovery = new SelfTestDiscovery();

            new TestAssembly(GetType().Assembly, listener).Discover(candidateTypes, discovery);

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
            var execution = new CreateInstancePerClass();

            new TestAssembly(GetType().Assembly, listener).Run(candidateTypes, discovery, execution);

            listener.Entries.ShouldBe(
                Self + "+PassTestClass.PassA passed",
                Self + "+PassTestClass.PassB passed",
                Self + "+PassFailTestClass.Fail failed: 'Fail' failed!",
                Self + "+PassFailTestClass.Pass passed",
                Self + "+SkipTestClass.SkipA skipped",
                Self + "+SkipTestClass.SkipB skipped");
        }

        public void ShouldAllowRandomShufflingOfCaseExecutionOrder()
        {
            var listener = new StubListener();

            var candidateTypes = new[]
            {
                typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int),
                typeof(PassFailTestClass), typeof(SkipTestClass)
            };
            var discovery = new SelfTestDiscovery();
            var execution = new CreateInstancePerClass();

            discovery.Methods
                .Shuffle(new Random(1));

            new TestAssembly(GetType().Assembly, listener).Run(candidateTypes, discovery, execution);

            listener.Entries.ShouldBe(
                Self + "+PassTestClass.PassB passed",
                Self + "+PassTestClass.PassA passed",
                Self + "+PassFailTestClass.Fail failed: 'Fail' failed!",
                Self + "+PassFailTestClass.Pass passed",
                Self + "+SkipTestClass.SkipB skipped",
                Self + "+SkipTestClass.SkipA skipped");
        }

        class CreateInstancePerClass : Execution
        {
            public void Execute(TestClass testClass)
            {
                var instance = testClass.Construct();

                testClass.RunTests(test =>
                {
                    if (test.Method.Name.Contains("Skip"))
                        return;
                    
                    test.RunCases(@case => @case.Execute(instance));
                });

                instance.Dispose();
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
                throw new Exception("Exception thrown while attempting to yield input parameters for method: " + method.Name);
            }
        }
    }
}