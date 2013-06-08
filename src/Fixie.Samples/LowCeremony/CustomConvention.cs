using System.Linq;
using Fixie.Conventions;

namespace Fixie.Samples.LowCeremony
{
    public class CustomConvention : Convention
    {
        static readonly string[] LifecycleMethods = new[] { "FixtureSetUp", "FixtureTearDown", "SetUp", "TearDown" };

        readonly MethodFilter fixtureSetUp = LifecycleMethod("FixtureSetUp");
        readonly MethodFilter fixtureTearDown = LifecycleMethod("FixtureTearDown");
        readonly MethodFilter setUp = LifecycleMethod("SetUp");
        readonly MethodFilter tearDown = LifecycleMethod("TearDown");

        public CustomConvention()
        {
            Fixtures
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Cases
                .Where(method => method.Void() || method.Async())
                .Where(method => LifecycleMethods.All(x => x != method.Name))
                .ZeroParameters();

            FixtureExecution
                .CreateInstancePerFixture();

            InstanceExecution
                .SetUpTearDown(fixtureSetUp, fixtureTearDown);

            CaseExecution
                .SetUpTearDown(setUp, tearDown);
        }

        static MethodFilter LifecycleMethod(string methodName)
        {
            return new MethodFilter().Where(x => x.HasSignature(typeof(void), methodName));
        }
    }
}