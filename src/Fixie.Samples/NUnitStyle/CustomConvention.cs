using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.Samples
{
    public delegate ExceptionList ClassAction(Type testClass);
    public delegate ExceptionList InstanceAction(Type testClass, object instance);

    public interface InstanceBehavior
    {
        void Execute(Type testClass, object instance, MethodInfo[] caseMethods, Dictionary<MethodInfo, ExceptionList> exceptionsByCase, Convention convention);
    }

    public interface LifecycleBehavior
    {
        void Execute(Type fixtureClass, MethodInfo[] caseMethods, Dictionary<MethodInfo, ExceptionList> exceptionsByCase, Convention convention);
    }

    public class EmitPassFail : TypeBehavior
    {
        readonly LifecycleBehavior lifecycleBehavior;

        public EmitPassFail(LifecycleBehavior lifecycleBehavior)
        {
            this.lifecycleBehavior = lifecycleBehavior;
        }

        public void Execute(Type fixtureClass, Convention convention, Listener listener)
        {
            var caseMethods = convention.CaseMethods(fixtureClass).ToArray();
            var exceptionsByCase = caseMethods.ToDictionary(x => x, x => new ExceptionList());

            lifecycleBehavior.Execute(fixtureClass, caseMethods, exceptionsByCase, convention);

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
    }

    public class ExecuteCases : InstanceBehavior
    {
        public void Execute(Type testClass, object instance, MethodInfo[] caseMethods, Dictionary<MethodInfo, ExceptionList> exceptionsByCase, Convention convention)
        {
            foreach (var caseMethod in caseMethods)
            {
                var exceptions = exceptionsByCase[caseMethod];
                convention.CaseExecutionBehavior.Execute(caseMethod, instance, exceptions);
            }
        }
    }

    public class InstancePerCase : LifecycleBehavior
    {
        readonly LifecycleBehavior inner;

        public InstancePerCase(LifecycleBehavior inner)
        {
            this.inner = inner;
        }

        public void Execute(Type fixtureClass, MethodInfo[] caseMethods, Dictionary<MethodInfo, ExceptionList> exceptionsByCase, Convention convention)
        {
            foreach (var caseMethod in caseMethods)
                inner.Execute(fixtureClass, new[] { caseMethod }, exceptionsByCase, convention);
        }
    }

    public class InstantiateAndExecuteCases : LifecycleBehavior
    {
        readonly InstanceBehavior inner;

        public InstantiateAndExecuteCases(InstanceBehavior inner)
        {
            this.inner = inner;
        }

        public void Execute(Type fixtureClass, MethodInfo[] caseMethods, Dictionary<MethodInfo, ExceptionList> exceptionsByCase, Convention convention)
        {
            object instance;
            var constructionExceptions = new ExceptionList();
            if (!TryConstruct(fixtureClass, constructionExceptions, out instance))
            {
                foreach (var caseMethod in caseMethods)
                    exceptionsByCase[caseMethod].Add(constructionExceptions);
            }
            else
            {
                inner.Execute(fixtureClass, instance, caseMethods, exceptionsByCase, convention);

                var disposalExceptions = Dispose(instance);
                if (disposalExceptions.Any())
                {
                    foreach (var caseMethod in caseMethods)
                        exceptionsByCase[caseMethod].Add(disposalExceptions);
                }
            }
        }

        static bool TryConstruct(Type fixtureClass, ExceptionList exceptions, out object instance)
        {
            try
            {
                instance = Activator.CreateInstance(fixtureClass);
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

    public class InstanceSetUpTearDown : InstanceBehavior
    {
        readonly InstanceAction setUp;
        readonly InstanceBehavior inner;
        readonly InstanceAction tearDown;

        public InstanceSetUpTearDown(InstanceAction setUp, InstanceBehavior inner, InstanceAction tearDown)
        {
            this.setUp = setUp;
            this.inner = inner;
            this.tearDown = tearDown;
        }

        public void Execute(Type testClass, object instance, MethodInfo[] caseMethods, Dictionary<MethodInfo, ExceptionList> exceptionsByCase, Convention convention)
        {
            var instanceSetUpExceptions = setUp(testClass, instance);
            if (instanceSetUpExceptions.Any())
            {
                foreach (var caseMethod in caseMethods)
                    exceptionsByCase[caseMethod].Add(instanceSetUpExceptions);
            }
            else
            {
                inner.Execute(testClass, instance, caseMethods, exceptionsByCase, convention);

                var instanceTearDownExceptions = tearDown(testClass, instance);
                if (instanceTearDownExceptions.Any())
                {
                    foreach (var caseMethod in caseMethods)
                        exceptionsByCase[caseMethod].Add(instanceTearDownExceptions);
                }
            }
        }
    }

    public class ClassSetUpTearDown : LifecycleBehavior
    {
        readonly ClassAction setUp;
        readonly LifecycleBehavior inner;
        readonly ClassAction tearDown;

        public ClassSetUpTearDown(ClassAction setUp, LifecycleBehavior inner, ClassAction tearDown)
        {
            this.setUp = setUp;
            this.inner = inner;
            this.tearDown = tearDown;
        }

        public void Execute(Type fixtureClass, MethodInfo[] caseMethods, Dictionary<MethodInfo, ExceptionList> exceptionsByCase, Convention convention)
        {
            var classSetUpExceptions = setUp(fixtureClass);
            if (classSetUpExceptions.Any())
            {
                foreach (var caseMethod in caseMethods)
                    exceptionsByCase[caseMethod].Add(classSetUpExceptions);
            }
            else
            {
                inner.Execute(fixtureClass, caseMethods, exceptionsByCase, convention);

                var classTearDownExceptions = tearDown(fixtureClass);
                foreach (var caseMethod in caseMethods)
                    exceptionsByCase[caseMethod].Add(classTearDownExceptions);
            }
        }
    }
}

namespace Fixie.Samples.NUnitStyle
{
    public class CustomConvention : Convention
    {
        readonly MethodFilter fixtureSetUps = new MethodFilter().HasOrInherits<TestFixtureSetUpAttribute>();
        readonly MethodFilter fixtureTearDowns = new MethodFilter().HasOrInherits<TestFixtureTearDownAttribute>();

        readonly MethodFilter setUps = new MethodFilter().HasOrInherits<SetUpAttribute>();
        readonly MethodFilter tearDowns = new MethodFilter().HasOrInherits<TearDownAttribute>();

        public CustomConvention()
        {
            Fixtures
                .HasOrInherits<TestFixtureAttribute>();

            Cases
                .HasOrInherits<TestAttribute>();

            CaseExecutionBehavior = new NUnitSetUpTearDown(setUps, CaseExecutionBehavior, tearDowns);

            FixtureExecutionBehavior =
                new EmitPassFail(
                    new InstantiateAndExecuteCases(
                        new InstanceSetUpTearDown(
                            InvokeAll(fixtureSetUps),
                            new ExecuteCases(),
                            InvokeAll(fixtureTearDowns)
                            )
                        )
                    );
        }

        static InstanceAction InvokeAll(MethodFilter methodFilter)
        {
            return (testClass, instance) => InvokeAll(testClass, instance, methodFilter);
        }

        static ExceptionList InvokeAll(Type fixtureClass, object instance, MethodFilter methodFilter)
        {
            var invoke = new Invoke();
            var exceptions = new ExceptionList();
            foreach (var method in methodFilter.Filter(fixtureClass))
                invoke.Execute(method, instance, exceptions);
            return exceptions;
        }
    }

    public class NUnitSetUpTearDown : MethodBehavior
    {
        readonly Invoke invoke;
        readonly MethodFilter setUps;
        readonly MethodBehavior inner;
        readonly MethodFilter tearDowns;

        public NUnitSetUpTearDown(MethodFilter setUps, MethodBehavior inner, MethodFilter tearDowns)
        {
            invoke = new Invoke();
            this.setUps = setUps;
            this.inner = inner;
            this.tearDowns = tearDowns;
        }

        public void Execute(MethodInfo method, object instance, ExceptionList exceptions)
        {
            var setUpExceptions = new ExceptionList();

            foreach (var setUp in setUps.Filter(method.ReflectedType))
                invoke.Execute(setUp, instance, setUpExceptions);

            if (setUpExceptions.Any())
            {
                exceptions.Add(setUpExceptions);
                return;
            }

            inner.Execute(method, instance, exceptions);

            foreach (var tearDown in tearDowns.Filter(method.ReflectedType))
                invoke.Execute(tearDown, instance, exceptions);
        }
    }
}