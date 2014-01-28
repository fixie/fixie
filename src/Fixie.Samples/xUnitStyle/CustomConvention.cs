namespace Fixie.Samples.xUnitStyle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Fixie;
    using Fixie.Conventions;
    using Xunit;
    using Xunit.Extensions;

    public class CustomConvention : Convention
    {
        private readonly Dictionary<MethodInfo, object> _fixtures = new Dictionary<MethodInfo, object>();

        public XUnitTestConvention()
        {
            Classes
                .Where(HasAnyFactOrTheoryMethods);

            Methods
                .Where(MethodHasAnyFactOrTheoryAttributes);

            ClassExecution
                .CreateInstancePerCase()
                .SetUpTearDown(PrepareFixtureData, DisposeFixtureData)
                .ShuffleCases();

            InstanceExecution
                .SetUp(FactOrTheoryMarkedAsSkip);

            CaseExecution
                .Skip(FactMarkedAsSkip);

            Parameters(FillFromDataAttributes);
        }

        private static bool MethodHasAnyFactOrTheoryAttributes(MethodInfo mi)
        {
            return mi.HasOrInherits<FactAttribute>() || mi.HasOrInherits<TheoryAttribute>();
        }

        private static IEnumerable<object[]> FillFromDataAttributes(MethodInfo arg)
        {
            var parameterTypes = arg.GetParameters().Select(p => p.ParameterType).ToArray();

            return arg.GetCustomAttributes<DataAttribute>().SelectMany(dataAttribute => dataAttribute.GetData(arg, parameterTypes));
        }

        private static bool FactOrTheoryMarkedAsSkip(Case arg)
        {
            return arg.Method.GetCustomAttributes<FactAttribute>().Any(attr => !string.IsNullOrEmpty(attr.Skip)) ||
                arg.Method.GetCustomAttributes<TheoryAttribute>().Any(attr => !string.IsNullOrEmpty(attr.Skip));
        }

        private static bool HasAnyFactOrTheoryMethods(Type type)
        {
            return new MethodFilter().Where(MethodHasAnyFactOrTheoryAttributes).Filter(type).Any();
        }

        private void PrepareFixtureData(Type testClass)
        {
            _fixtures.Clear();

            foreach (var @interface in FixtureInterfaces(testClass))
            {
                var fixtureDataType = @interface.GetGenericArguments()[0];

                var fixtureInstance = Activator.CreateInstance(fixtureDataType);

                var method = @interface.GetMethod("SetFixture", new[] { fixtureDataType });
                _fixtures[method] = fixtureInstance;
            }
        }

        private void DisposeFixtureData(Type testClass)
        {
            foreach (var fixtureInstance in _fixtures.Values)
            {
                var disposable = fixtureInstance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            _fixtures.Clear();
        }

        private void InjectFixtureData(Fixture fixture)
        {
            foreach (var injectionMethod in _fixtures.Keys)
                injectionMethod.Invoke(fixture.Instance, new[] { _fixtures[injectionMethod] });
        }

        private static IEnumerable<Type> FixtureInterfaces(Type testClass)
        {
            return testClass.GetInterfaces()
                            .Where(@interface => @interface.IsGenericType &&
                                                 @interface.GetGenericTypeDefinition() == typeof(IUseFixture<>));
        }
    }
}
