using System;
using System.Linq;
using System.Threading.Tasks;
using Fixie.Conventions;

namespace Fixie.Tests.Conventions
{
    public class ConventionTests
    {
        public void EmptyConventionShouldDiscoverConcreteClassesAsTestClasses()
        {
            var emptyConvention = new Convention();

            emptyConvention.Classes
                           .Filter(CandidateTypes)
                           .Select(x => x.Name)
                           .ShouldEqual("PublicTests", "OtherPublicTests", "PublicMissingNamingConvention", "PublicWithNoDefaultConstructorTests",
                                        "PrivateTests", "OtherPrivateTests", "PrivateMissingNamingConvention", "PrivateWithNoDefaultConstructorTests");
        }

        public void DefaultConventionShouldDiscoverConcreteClassesFollowingNamingConventionAsTestClasses()
        {
            var defaultConvention = new DefaultConvention();

            defaultConvention.Classes
                             .Filter(CandidateTypes)
                             .Select(x => x.Name)
                             .ShouldEqual("PublicTests", "OtherPublicTests", "PublicWithNoDefaultConstructorTests",
                                          "PrivateTests", "OtherPrivateTests", "PrivateWithNoDefaultConstructorTests");
        }

        static Type[] CandidateTypes
        {
            get
            {
                return new[]
                {
                    typeof(PublicInterfaceTests),
                    typeof(PublicAbstractTests),
                    typeof(PublicTests),
                    typeof(OtherPublicTests),
                    typeof(PublicMissingNamingConvention),
                    typeof(PublicWithNoDefaultConstructorTests),
                    typeof(PrivateInterfaceTests),
                    typeof(PrivateAbstractTests),
                    typeof(PrivateTests),
                    typeof(OtherPrivateTests),
                    typeof(PrivateMissingNamingConvention),
                    typeof(PrivateWithNoDefaultConstructorTests)
                };
            }
        }

        public interface PublicInterfaceTests { }
        public abstract class PublicAbstractTests { }
        public class PublicTests : PublicAbstractTests { }
        public class OtherPublicTests { }
        public class PublicMissingNamingConvention { }
        public class PublicWithNoDefaultConstructorTests { public PublicWithNoDefaultConstructorTests(int x) { } }

        interface PrivateInterfaceTests { }
        abstract class PrivateAbstractTests { }
        class PrivateTests : PrivateAbstractTests { }
        class OtherPrivateTests { }
        class PrivateMissingNamingConvention { }
        class PrivateWithNoDefaultConstructorTests { public PrivateWithNoDefaultConstructorTests(int x) { } }

        public void EmptyConventionShouldDiscoverPublicInstanceMethodsForTestCases()
        {
            var emptyConvention = new Convention();
            var testClass = typeof(DiscoveryTestClass);

            emptyConvention.Methods
                           .Filter(testClass)
                           .OrderBy(x => x.Name)
                           .Select(x => x.Name)
                           .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceNoArgsWithReturn",
                                        "PublicInstanceWithArgsVoid", "PublicInstanceWithArgsWithReturn");
        }

        public void DefaultConventionShouldDiscoverSynchronousPublicInstanceVoidMethodsForTestCases()
        {
            var defaultConvention = new DefaultConvention();
            var testClass = typeof(DiscoveryTestClass);

            defaultConvention.Methods
                             .Filter(testClass)
                             .OrderBy(x => x.Name)
                             .Select(x => x.Name)
                             .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceWithArgsVoid");
        }

        public void DefaultConventionShouldDiscoverAsyncPublicInstanceMethodsForTestCases()
        {
            var defaultConvention = new DefaultConvention();
            var testClass = typeof(AsyncDiscoveryTestClass);

            defaultConvention.Methods
                             .Filter(testClass)
                             .OrderBy(x => x.Name)
                             .Select(x => x.Name)
                             .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceNoArgsWithReturn",
                                          "PublicInstanceWithArgsVoid", "PublicInstanceWithArgsWithReturn");
        }

        class DiscoveryTestClass : IDisposable
        {
            public static int PublicStaticWithArgsWithReturn(int x) { return 0; }
            public static int PublicStaticNoArgsWithReturn() { return 0; }
            public static void PublicStaticWithArgsVoid(int x) { }
            public static void PublicStaticNoArgsVoid() { }

            public int PublicInstanceWithArgsWithReturn(int x) { return 0; }
            public int PublicInstanceNoArgsWithReturn() { return 0; }
            public void PublicInstanceWithArgsVoid(int x) { }
            public void PublicInstanceNoArgsVoid() { }

            private static int PrivateStaticWithArgsWithReturn(int x) { return 0; }
            private static int PrivateStaticNoArgsWithReturn() { return 0; }
            private static void PrivateStaticWithArgsVoid(int x) { }
            private static void PrivateStaticNoArgsVoid() { }

            private int PrivateInstanceWithArgsWithReturn(int x) { return 0; }
            private int PrivateInstanceNoArgsWithReturn() { return 0; }
            private void PrivateInstanceWithArgsVoid(int x) { }
            private void PrivateInstanceNoArgsVoid() { }

            public void Dispose() { }
        }

        class AsyncDiscoveryTestClass
        {
            public async static Task<int> PublicStaticWithArgsWithReturn(int x) { return await Zero(); }
            public async static Task<int> PublicStaticNoArgsWithReturn() { return await Zero(); }
            public async static void PublicStaticWithArgsVoid(int x) { await Zero(); }
            public async static void PublicStaticNoArgsVoid() { await Zero(); }

            public async Task<int> PublicInstanceWithArgsWithReturn(int x) { return await Zero(); }
            public async Task<int> PublicInstanceNoArgsWithReturn() { return await Zero(); }
            public async void PublicInstanceWithArgsVoid(int x) { await Zero(); }
            public async void PublicInstanceNoArgsVoid() { await Zero(); }

            private async static Task<int> PrivateStaticWithArgsWithReturn(int x) { return await Zero(); }
            private async static Task<int> PrivateStaticNoArgsWithReturn() { return await Zero(); }
            private async static void PrivateStaticWithArgsVoid(int x) { await Zero(); }
            private async static void PrivateStaticNoArgsVoid() { await Zero(); }

            private async Task<int> PrivateInstanceWithArgsWithReturn(int x) { return await Zero(); }
            private async Task<int> PrivateInstanceNoArgsWithReturn() { return await Zero(); }
            private async void PrivateInstanceWithArgsVoid(int x) { await Zero(); }
            private async void PrivateInstanceNoArgsVoid() { await Zero(); }

            static Task<int> Zero()
            {
                return Task.Run(() => 0);
            }
        }

        public void ShouldExecuteAllCasesInAllDiscoveredTestClasses()
        {
            var listener = new StubListener();
            var convention = new SelfTestConvention();

            convention.Execute(listener, typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int), typeof(PassFailTestClass), typeof(SkipTestClass));

            listener.Entries.ShouldEqual("Fixie.Tests.Conventions.ConventionTests+PassTestClass.PassA passed.",
                                         "Fixie.Tests.Conventions.ConventionTests+PassTestClass.PassB passed.",
                                         "Fixie.Tests.Conventions.ConventionTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                                         "Fixie.Tests.Conventions.ConventionTests+PassFailTestClass.Pass passed.",
                                         "Fixie.Tests.Conventions.ConventionTests+SkipTestClass.Skip skipped.");
        }

        public void ShouldAllowRandomShufflingOfCaseExecutionOrder()
        {
            var listener = new StubListener();
            var convention = new SelfTestConvention();

            convention.ClassExecution
                      .CreateInstancePerTestClass()
                      .ShuffleCases(new Random(1));

            convention.Execute(listener, typeof(SampleIrrelevantClass), typeof(PassTestClass), typeof(int), typeof(PassFailTestClass), typeof(SkipTestClass));

            listener.Entries.ShouldEqual("Fixie.Tests.Conventions.ConventionTests+PassTestClass.PassB passed.",
                                         "Fixie.Tests.Conventions.ConventionTests+PassTestClass.PassA passed.",
                                         "Fixie.Tests.Conventions.ConventionTests+PassFailTestClass.Fail failed: 'Fail' failed!",
                                         "Fixie.Tests.Conventions.ConventionTests+PassFailTestClass.Pass passed.",
                                         "Fixie.Tests.Conventions.ConventionTests+SkipTestClass.Skip skipped.");
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