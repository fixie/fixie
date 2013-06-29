using System;
using System.Linq;
using System.Threading.Tasks;
using Fixie.Conventions;

namespace Fixie.Tests.Conventions
{
    public class ConventionTests
    {
        public void EmptyConventionShouldTreatConcreteClassesAsTestClasses()
        {
            var emptyConvention = new Convention();

            emptyConvention.Classes
                           .Filter(CandidateTypes)
                           .Select(x => x.Name)
                           .ShouldEqual("PublicTests", "OtherPublicTests", "PublicMissingNamingConvention", "PublicWithNoDefaultConstructorTests",
                                        "PrivateTests", "OtherPrivateTests", "PrivateMissingNamingConvention", "PrivateWithNoDefaultConstructorTests");
        }

        public void DefaultConventionShouldTreatConcreteClassesFollowingNamingConventionAsTestClasses()
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

        public void EmptyConventionShouldTreatPublicInstanceMethodsAsCases()
        {
            var emptyConvention = new Convention();
            var fixtureClass = typeof(DiscoveryFixture);

            emptyConvention.Cases
                           .Filter(fixtureClass)
                           .OrderBy(x => x.Name)
                           .Select(x => x.Name)
                           .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceNoArgsWithReturn",
                                        "PublicInstanceWithArgsVoid", "PublicInstanceWithArgsWithReturn");
        }

        public void DefaultConventionShouldTreatSynchronousPublicInstanceNoArgVoidMethodsAsCases()
        {
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(DiscoveryFixture);

            defaultConvention.Cases
                             .Filter(fixtureClass)
                             .Select(x => x.Name)
                             .ShouldEqual("PublicInstanceNoArgsVoid");
        }

        public void DefaultConventionShouldTreatAsyncPublicInstanceNoArgMethodsAsCases()
        {
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(AsyncDiscoveryFixture);

            defaultConvention.Cases
                             .Filter(fixtureClass)
                             .OrderBy(x => x.Name)
                             .Select(x => x.Name)
                             .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceNoArgsWithReturn");
        }

        class DiscoveryFixture : IDisposable
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

        class AsyncDiscoveryFixture
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

            convention.Execute(listener, typeof(SampleIrrelevantClass), typeof(PassFixture), typeof(int), typeof(PassFailFixture));

            listener.ShouldHaveEntries("Fixie.Tests.Conventions.ConventionTests+PassFailFixture.Pass passed.",
                                       "Fixie.Tests.Conventions.ConventionTests+PassFailFixture.Fail failed: 'Fail' failed!",
                                       "Fixie.Tests.Conventions.ConventionTests+PassFixture.PassA passed.",
                                       "Fixie.Tests.Conventions.ConventionTests+PassFixture.PassB passed.");
        }

        class SampleIrrelevantClass
        {
            public void PassA() { }
            public void PassB() { }
        }

        class PassFixture
        {
            public void PassA() { }
            public void PassB() { }
        }

        class PassFailFixture
        {
            public void Pass() { }
            public void Fail() { throw new FailureException(); }
        }
    }
}