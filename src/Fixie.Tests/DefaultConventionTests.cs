using System.Linq;
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

            var convention = new DefaultConvention();

            convention.FixtureClasses(candidateTypes)
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
        public void ShouldTreatPublicInstanceNoArgVoidMethodsAsCases()
        {
            var convention = new DefaultConvention();
            var fixtureClass = typeof(DiscoverySampleFixture);

            convention.CaseMethods(fixtureClass)
                        .Select(x => x.Name)
                        .ShouldEqual("PublicInstanceNoArgsVoid");
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
    }
}