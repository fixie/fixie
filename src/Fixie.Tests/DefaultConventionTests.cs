using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fixie.Tests
{
    public class DefaultConventionTests
    {
        [Fact]
        public void ShouldTreatConstructibleClassesFollowingNamingConventionAsFixtures()
        {
            var candidateTypes = new[]
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

            var defaultConvention = new DefaultConvention();

            defaultConvention.FixtureClasses(candidateTypes)
                             .Select(x => x.Name)
                             .ShouldEqual("PublicTests", "OtherPublicTests", "PrivateTests", "OtherPrivateTests");
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

        [Fact]
        public void ShouldTreatSynchronousPublicInstanceNoArgVoidMethodsAsCases()
        {
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(DiscoverySampleFixture);

            defaultConvention.CaseMethods(fixtureClass)
                             .Select(x => x.Name)
                             .ShouldEqual("PublicInstanceNoArgsVoid");
        }

        [Fact]
        public void ShouldTreatAsyncPublicInstanceNoArgMethodsAsCases()
        {
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(AsyncDiscoverySampleFixture);

            defaultConvention.CaseMethods(fixtureClass)
                             .OrderBy(x => x.Name)
                             .Select(x => x.Name)
                             .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceNoArgsWithReturn");
        }

        class DiscoverySampleFixture
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
        }

        class AsyncDiscoverySampleFixture
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
    }
}