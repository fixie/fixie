using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Samples.xUnitStyle
{
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
                .CreateInstancePerCase()
                .Wrap<PrepareAndDisposeFixtureData>()
                .ShuffleCases();

            InstanceExecution
                .Wrap<InjectFixtureData>();
        }

        bool HasAnyFactMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(x => x.HasOrInherits<FactAttribute>());
        }

        class PrepareAndDisposeFixtureData : ClassBehavior
        {
            public void Execute(ClassExecution classExecution, Action next)
            {
                SetUp(classExecution);
                next();
                TearDown();
            }

            void SetUp(ClassExecution classExecution)
            {
                fixtures.Clear();

                foreach (var @interface in FixtureInterfaces(classExecution.TestClass))
                {
                    var fixtureDataType = @interface.GetGenericArguments()[0];

                    var fixtureInstance = Activator.CreateInstance(fixtureDataType);

                    var method = @interface.GetMethod("SetFixture", new[] { fixtureDataType });
                    fixtures[method] = fixtureInstance;
                }
            }

            void TearDown()
            {
                foreach (var fixtureInstance in fixtures.Values)
                {
                    var disposable = fixtureInstance as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }

                fixtures.Clear();
            }
        }

        class InjectFixtureData : InstanceBehavior
        {
            public void Execute(InstanceExecution instanceExecution, Action next)
            {
                foreach (var injectionMethod in fixtures.Keys)
                    injectionMethod.Invoke(instanceExecution.Instance, new[] { fixtures[injectionMethod] });

                next();
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