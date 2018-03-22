namespace Fixie.Samples.xUnitStyle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .Where(HasAnyFactMethods);

            Methods
                .Where(x => x.HasOrInherits<FactAttribute>())
                .Shuffle();
        }

        public override void Execute(TestClass testClass, Action<CaseAction> runCases)
        {
            var fixtures = PrepareFixtureData(testClass.Type);

            runCases(@case =>
            {
                var instance = testClass.Construct();

                foreach (var injectionMethod in fixtures.Keys)
                    injectionMethod.Invoke(instance, new[] { fixtures[injectionMethod] });

                @case.Execute(instance);

                instance.Dispose();
            });

            foreach (var fixtureInstance in fixtures.Values)
                fixtureInstance.Dispose();
        }

        static Dictionary<MethodInfo, object> PrepareFixtureData(Type testClass)
        {
            var fixtures = new Dictionary<MethodInfo, object>();

            foreach (var @interface in FixtureInterfaces(testClass))
            {
                var fixtureDataType = @interface.GetGenericArguments()[0];

                var fixtureInstance = Activator.CreateInstance(fixtureDataType);

                var method = @interface.GetMethod("SetFixture", new[] { fixtureDataType });
                fixtures[method] = fixtureInstance;
            }

            return fixtures;
        }

        bool HasAnyFactMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Any(x => x.HasOrInherits<FactAttribute>());
        }

        static IEnumerable<Type> FixtureInterfaces(Type testClass)
        {
            return testClass.GetInterfaces()
                            .Where(@interface => @interface.IsGenericType &&
                                                 @interface.GetGenericTypeDefinition() == typeof(IUseFixture<>));
        }
    }
}