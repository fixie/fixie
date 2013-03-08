using System.Linq;
using NUnit.Framework;

namespace Fixie.Tests
{
    [TestFixture]
    public class FixtureTests
    {
        [Test]
        public void ShouldTreatPublicInstanceNoArgVoidMethodsAsCases()
        {
            var fixtureClass = typeof(SampleFixture);

            var fixture = new Fixture(fixtureClass);

            var cases = fixture.Cases;

            cases.Select(x => x.Name).ShouldBe("PublicInstanceNoArgsVoid");
        }

        class SampleFixture
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