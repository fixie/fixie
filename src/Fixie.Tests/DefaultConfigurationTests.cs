using System.Linq;
using NUnit.Framework;

namespace Fixie.Tests
{
    [TestFixture]
    public class DefaultConfigurationTests
    {
        [Test]
        public void ShouldTreatConstructibleClassesFollowingNamingConventionAsFixtures()
        {
            var configuration = new DefaultConfiguration(
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
                typeof(PrivateWithNoDefaultConstructorTests));

            var fixtures = configuration.Fixtures;

            fixtures.Select(x => x.Name).ShouldBe("PublicTests", "OtherPublicTests", "PrivateTests", "OtherPrivateTests");
        }

        public interface PublicInterfaceTests { }
        public abstract class PublicAbstractTests { }
        public class PublicTests : PublicAbstractTests { }
        public class OtherPublicTests { }
        public class PublicMissingNamingConvention { }
        public class PublicWithNoDefaultConstructorTests { public PublicWithNoDefaultConstructorTests(int x) { } }

        private interface PrivateInterfaceTests { }
        private abstract class PrivateAbstractTests { }
        private class PrivateTests : PrivateAbstractTests { }
        private class OtherPrivateTests { }
        private class PrivateMissingNamingConvention { }
        private class PrivateWithNoDefaultConstructorTests { public PrivateWithNoDefaultConstructorTests(int x) {  } }
    }
}