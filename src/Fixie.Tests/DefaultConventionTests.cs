using System.Linq;
using Xunit;

namespace Fixie.Tests
{
    public class DefaultConventionTests
    {
        [Fact]
        public void ShouldTreatConstructibleClassesFollowingNamingConventionAsFixtures()
        {
            var convention = new DefaultConvention(
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

            var fixtures = convention.Fixtures;

            fixtures.Select(x => x.Name).ShouldEqual(
                "Fixie.Tests.DefaultConventionTests+PublicTests",
                "Fixie.Tests.DefaultConventionTests+OtherPublicTests",
                "Fixie.Tests.DefaultConventionTests+PrivateTests",
                "Fixie.Tests.DefaultConventionTests+OtherPrivateTests");
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