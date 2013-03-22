using System.Linq;
using Should;
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

            convention.Fixtures.Select(x => x.Name).ShouldEqual(
                "Fixie.Tests.DefaultConventionTests+PublicTests",
                "Fixie.Tests.DefaultConventionTests+OtherPublicTests",
                "Fixie.Tests.DefaultConventionTests+PrivateTests",
                "Fixie.Tests.DefaultConventionTests+OtherPrivateTests");
        }

        [Fact]
        public void ShouldExecuteAllCasesInAllFixtures()
        {
            var listener = new StubListener();
            var convention = new DefaultConvention(typeof(ExecutionSampleTests));

            var result = convention.Execute(listener);

            result.Total.ShouldEqual(2);
            result.Passed.ShouldEqual(2);
            result.Failed.ShouldEqual(0);

            listener.Entries.ShouldBeEmpty();
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

        public class ExecutionSampleTests
        {
            [Fact]
            public void PassA() { }

            [Fact]
            public void PassB() { }
        }
    }
}