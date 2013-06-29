using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Behaviors;
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
                .Where(HasAnyFactMethods);

            Cases
                .HasOrInherits<FactAttribute>();

            ClassExecution
                .CreateInstancePerCase()
                .SetUpTearDown(PrepareFixtureData, DisposeFixtureData);

            InstanceExecution
                .SetUpTearDown(InjectFixtureData, fixture => new ExceptionList());
        }

        bool HasAnyFactMethods(Type type)
        {
            return factMethods.Filter(type).Any();
        }

        ExceptionList PrepareFixtureData(Type testClass)
        {
            var exceptions = new ExceptionList();

            foreach (var @interface in FixtureInterfaces(testClass))
            {
                var fixtureDataType = @interface.GetGenericArguments()[0];

                object fixtureInstance;

                var constructionExceptions = Lifecycle.Construct(fixtureDataType, out fixtureInstance);

                if (constructionExceptions.Any())
                {
                    exceptions.Add(constructionExceptions);
                }
                else
                {
                    var method = @interface.GetMethod("SetFixture", new[] { fixtureDataType });
                    fixtures[method] = fixtureInstance;
                }
            }

            return exceptions;
        }

        ExceptionList DisposeFixtureData(Type testClass)
        {
            var classTearDownExceptions = new ExceptionList();
            foreach (var fixtureInstance in fixtures.Values)
            {
                var disposalExceptions = Lifecycle.Dispose(fixtureInstance);

                classTearDownExceptions.Add(disposalExceptions);
            }
            return classTearDownExceptions;
        }

        ExceptionList InjectFixtureData(Fixture fixture)
        {
            var exceptions = new ExceptionList();

            foreach (var injectionMethod in fixtures.Keys)
            {
                try
                {
                    injectionMethod.Invoke(fixture.Instance, new[] { fixtures[injectionMethod] });
                }
                catch (TargetInvocationException ex)
                {
                    exceptions.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            return exceptions;
        }

        static IEnumerable<Type> FixtureInterfaces(Type testClass)
        {
            return testClass.GetInterfaces()
                            .Where(@interface => @interface.IsGenericType &&
                                                 @interface.GetGenericTypeDefinition() == typeof(IUseFixture<>));
        }
    }
}