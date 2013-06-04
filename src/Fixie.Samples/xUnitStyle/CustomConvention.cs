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
            Fixtures
                .Where(HasAnyFactMethods);

            Cases
                .HasOrInherits<FactAttribute>();

            FixtureExecution
                .CreateInstancePerCase()
                .SetUpTearDown(PrepareFixtureData, DisposeFixtureData);

            InstanceExecution
                .SetUpTearDown(InjectFixtureData, (fixtureClass, instance) => new ExceptionList());
        }

        bool HasAnyFactMethods(Type type)
        {
            return factMethods.Filter(type).Any();
        }

        ExceptionList PrepareFixtureData(Type fixtureClass)
        {
            var exceptions = new ExceptionList();

            foreach (var @interface in FixtureInterfaces(fixtureClass))
            {
                var fixtureDataType = @interface.GetGenericArguments()[0];

                object fixtureInstance;

                if (TryConstruct(fixtureDataType, exceptions, out fixtureInstance))
                {
                    var method = @interface.GetMethod("SetFixture", new[] { fixtureDataType });
                    fixtures[method] = fixtureInstance;
                }
            }

            return exceptions;
        }

        ExceptionList DisposeFixtureData(Type fixtureClass)
        {
            var classTearDownExceptions = new ExceptionList();
            foreach (var fixtureInstance in fixtures.Values)
            {
                var disposalExceptions = Dispose(fixtureInstance);

                classTearDownExceptions.Add(disposalExceptions);
            }
            return classTearDownExceptions;
        }

        ExceptionList InjectFixtureData(Type fixtureClass, object instance)
        {
            var exceptions = new ExceptionList();

            foreach (var injectionMethod in fixtures.Keys)
            {
                try
                {
                    injectionMethod.Invoke(instance, new[] { fixtures[injectionMethod] });
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

        static IEnumerable<Type> FixtureInterfaces(Type fixtureClass)
        {
            return fixtureClass.GetInterfaces()
                            .Where(@interface => @interface.IsGenericType &&
                                                 @interface.GetGenericTypeDefinition() == typeof(IUseFixture<>));
        }

        static bool TryConstruct(Type @class, ExceptionList exceptions, out object instance)
        {
            try
            {
                instance = Activator.CreateInstance(@class);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                exceptions.Add(ex.InnerException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            instance = null;
            return false;
        }

        static ExceptionList Dispose(object instance)
        {
            var exceptions = new ExceptionList();
            try
            {
                var disposable = instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            return exceptions;
        }
    }
}