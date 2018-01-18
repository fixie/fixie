namespace Fixie.Samples.xUnitStyle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CustomConvention : Convention
    {
        static readonly Dictionary<MethodInfo, object> fixtures = new Dictionary<MethodInfo, object>();

        public CustomConvention()
        {
            Classes
                .Where(HasAnyFactMethods);

            Methods
                .HasOrInherits<FactAttribute>();

            ClassExecution
                .Lifecycle<FixtureDataLifecycle>()
                .ShuffleCases();
        }

        bool HasAnyFactMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(x => x.HasOrInherits<FactAttribute>());
        }

        class FixtureDataLifecycle : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                PrepareFixtureData(testClass);
                runCases(@case =>
                {
                    var instance = Activator.CreateInstance(@case.Class);

                    foreach (var injectionMethod in fixtures.Keys)
                        injectionMethod.Invoke(instance, new[] { fixtures[injectionMethod] });

                    @case.Execute(instance);

                    (instance as IDisposable)?.Dispose();
                });
                DisposeFixtureData();
            }

            static void PrepareFixtureData(Type testClass)
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

            static void DisposeFixtureData()
            {
                foreach (var fixtureInstance in fixtures.Values)
                    (fixtureInstance as IDisposable)?.Dispose();

                fixtures.Clear();
            }
        }

        static IEnumerable<Type> FixtureInterfaces(Type testClass)
        {
            return testClass.GetInterfaces()
                            .Where(@interface => @interface.IsGenericType &&
                                                 @interface.GetGenericTypeDefinition() == typeof(IUseFixture<>));
        }
    }
}