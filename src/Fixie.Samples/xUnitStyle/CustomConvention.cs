using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Samples.xUnitStyle
{
    public class CustomConvention : Convention
    {
        readonly MethodFilter factMethods = new MethodFilter().HasOrInherits<FactAttribute>();
        readonly Dictionary<MethodInfo, object> fixtures = new Dictionary<MethodInfo, object>();

        public CustomConvention()
        {
            Classes
                .Where(x => factMethods.Filter(x).Any());

            Cases = new MethodFilter()
                .HasOrInherits<FactAttribute>();

            ClassExecution
                .CreateInstancePerCase()
                .SetUpTearDown(PrepareFixtureData, DisposeFixtureData);

            InstanceExecution
                .SetUpTearDown(InjectFixtureData, fixture => { });
        }

        void PrepareFixtureData(Type testClass)
        {
            fixtures.Clear();

            foreach (var @interface in FixtureInterfaces(testClass))
            {
                var fixtureDataType = @interface.GetGenericArguments()[0];

                var fixtureInstance = Activator.CreateInstance(fixtureDataType);

                var method = @interface.GetMethod("SetFixture", new[] { fixtureDataType });
                fixtures[method] = fixtureInstance;
            }
        }

        void DisposeFixtureData(Type testClass)
        {
            foreach (var fixtureInstance in fixtures.Values)
            {
                var disposable = fixtureInstance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            fixtures.Clear();
        }

        void InjectFixtureData(Fixture fixture)
        {
            foreach (var injectionMethod in fixtures.Keys)
                injectionMethod.Invoke(fixture.Instance, new[] { fixtures[injectionMethod] });
        }

        static IEnumerable<Type> FixtureInterfaces(Type testClass)
        {
            return testClass.GetInterfaces()
                            .Where(@interface => @interface.IsGenericType &&
                                                 @interface.GetGenericTypeDefinition() == typeof(IUseFixture<>));
        }
    }
}