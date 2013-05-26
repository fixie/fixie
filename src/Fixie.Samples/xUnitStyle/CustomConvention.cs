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

        public CustomConvention()
        {
            Fixtures
                .Where(HasAnyFactMethods);

            Cases
                .HasOrInherits<FactAttribute>();


            var fixtures = new Dictionary<MethodInfo, object>();
            ClassAction ClassSetUp = testClass => PrepareFixtureData(testClass, fixtures);
            ClassAction ClassTearDown = testClass => DisposeFixtureData(fixtures);
            InstanceAction InstanceSetUp = (testClass, instance) => InjectFixtureData(instance, fixtures);
            InstanceAction InstanceTearDown = (testClass, instance) => new ExceptionList();

            FixtureExecutionBehavior =
                new ClassSetUpTearDown(
                    ClassSetUp,
                    new InstancePerCase(
                        new InstantiateAndExecuteCases(
                            new InstanceSetUpTearDown(
                                InstanceSetUp,
                                new ExecuteCases(),
                                InstanceTearDown)
                            )
                        ),
                    ClassTearDown
                    );
        }

        bool HasAnyFactMethods(Type type)
        {
            return factMethods.Filter(type).Any();
        }

        static ExceptionList PrepareFixtureData(Type testClass, Dictionary<MethodInfo, object> fixtures)
        {
            var exceptions = new ExceptionList();

            foreach (var @interface in FixtureInterfaces(testClass))
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

        static ExceptionList DisposeFixtureData(Dictionary<MethodInfo, object> fixtures)
        {
            var classTearDownExceptions = new ExceptionList();
            foreach (var fixtureInstance in fixtures.Values)
            {
                var disposalExceptions = Dispose(fixtureInstance);

                classTearDownExceptions.Add(disposalExceptions);
            }
            return classTearDownExceptions;
        }

        static ExceptionList InjectFixtureData(object instance, Dictionary<MethodInfo, object> fixtures)
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

        static IEnumerable<Type> FixtureInterfaces(Type testClass)
        {
            return testClass.GetInterfaces()
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