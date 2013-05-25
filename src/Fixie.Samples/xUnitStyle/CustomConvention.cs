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

        public CustomConvention()
        {
            Fixtures
                .Where(HasAnyFactMethods);

            Cases
                .HasOrInherits<FactAttribute>();

            FixtureExecutionBehavior = new CreateInstancePerCaseWithFixtureData();
        }

        bool HasAnyFactMethods(Type type)
        {
            return factMethods.Filter(type).Any();
        }
    }

    public class CreateInstancePerCaseWithFixtureData : TypeBehavior
    {
        public void Execute(Type fixtureClass, Convention convention, Listener listener)
        {
            var caseMethods = convention.CaseMethods(fixtureClass).ToArray();
            var exceptionsByCase = caseMethods.ToDictionary(x => x, x => new ExceptionList());

            var classSetUpExceptions = new ExceptionList();
            var fixtures = SetUpFixtureInstances(fixtureClass, classSetUpExceptions);

            if (classSetUpExceptions.Any())
            {
                foreach (var caseMethod in caseMethods)
                    exceptionsByCase[caseMethod].Add(classSetUpExceptions);
            }
            else
            {
                foreach (var caseMethod in caseMethods)
                {
                    var exceptions = exceptionsByCase[caseMethod];

                    object instance;

                    if (TryConstruct(fixtureClass, exceptions, out instance))
                    {
                        bool injectionFailed = false;
                        foreach (var injectionMethod in fixtures.Keys)
                        {
                            try
                            {
                                injectionMethod.Invoke(instance, new[] { fixtures[injectionMethod] });
                            }
                            catch (TargetInvocationException ex)
                            {
                                exceptions.Add(ex.InnerException);
                                injectionFailed = true;
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                                injectionFailed = true;
                            }
                        }

                        if (!injectionFailed)
                            convention.CaseExecutionBehavior.Execute(caseMethod, instance, exceptions);

                        Dispose(instance, exceptions);
                    }
                }

                var classTearDownExceptions = new ExceptionList();
                foreach (var fixtureInstance in fixtures.Values)
                    Dispose(fixtureInstance, classTearDownExceptions);
                foreach (var caseMethod in caseMethods)
                    exceptionsByCase[caseMethod].Add(classTearDownExceptions);
            }

            foreach (var caseMethod in caseMethods)
            {
                var @case = fixtureClass.FullName + "." + caseMethod.Name;
                var exceptions = exceptionsByCase[caseMethod];

                if (exceptions.Any())
                    listener.CaseFailed(@case, exceptions.ToArray());
                else
                    listener.CasePassed(@case);
            }
        }

        static Dictionary<MethodInfo, object> SetUpFixtureInstances(Type testClass, ExceptionList exceptions)
        {
            var fixtures = new Dictionary<MethodInfo, object>();

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

            return fixtures;
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

        static void Dispose(object instance, ExceptionList exceptions)
        {
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
        }
    }
}