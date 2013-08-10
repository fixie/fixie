using System;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Samples.LowCeremony
{
    public class CustomConvention : Convention
    {
        static readonly string[] LifecycleMethods = new[] { "FixtureSetUp", "FixtureTearDown", "SetUp", "TearDown" };

        public CustomConvention()
        {
            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Cases
                .Where(method => method.Void())
                .Where(method => LifecycleMethods.All(x => x != method.Name));

            ClassExecution
                .CreateInstancePerTestClass();

            InstanceExecution
                .SetUpTearDown("FixtureSetUp", "FixtureTearDown");

            CaseExecution
                .SetUpTearDown("SetUp", "TearDown");
        }
    }

    public static class BehaviorBuilderExtensions
    {
        public static InstanceBehaviorBuilder SetUpTearDown(this InstanceBehaviorBuilder builder, string setUpMethod, string tearDownMethod)
        {
            return builder.SetUpTearDown(fixture => TryInvoke(setUpMethod, fixture.TestClass, fixture.Instance),
                                         fixture => TryInvoke(tearDownMethod, fixture.TestClass, fixture.Instance));
        }

        public static CaseBehaviorBuilder SetUpTearDown(this CaseBehaviorBuilder builder, string setUpMethod, string tearDownMethod)
        {
            return builder.SetUpTearDown((@case, instance) => TryInvoke(setUpMethod, @case.Class, instance),
                                         (@case, instance) => TryInvoke(tearDownMethod, @case.Class, instance));
        }

        static void TryInvoke(string method, Type type, object instance)
        {
            var lifecycleMethod =
                new MethodFilter()
                    .Where(x => x.HasSignature(typeof(void), method))
                    .Filter(type)
                    .SingleOrDefault();

            if (lifecycleMethod == null)
                return;

            try
            {
                lifecycleMethod.Invoke(instance, null);
            }
            catch (TargetInvocationException ex)
            {
                throw new PreservedException(ex.InnerException);
            }
        }
    }
}