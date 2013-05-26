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
        void Execute(Type testClass, object instance, Case[] cases, Convention convention);
    }

    public interface LifecycleBehavior
    {
        void Execute(Type fixtureClass, Case[] cases, Convention convention);
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
            var cases = convention.CaseMethods(fixtureClass).Select(x => new Case(fixtureClass, x)).ToArray();

            lifecycleBehavior.Execute(fixtureClass, cases, convention);

            foreach (var @case in cases)
            {
                var exceptions = @case.Exceptions;

                if (exceptions.Any())
                    listener.CaseFailed(@case.Name, exceptions.ToArray());
                else
                    listener.CasePassed(@case.Name);
            }
        }
    }

    public class ExecuteCases : InstanceBehavior
    {
        public void Execute(Type testClass, object instance, Case[] cases, Convention convention)
        {
            foreach (var @case in cases)
                convention.CaseExecutionBehavior.Execute(@case.Method, instance, @case.Exceptions);
        }
    }

    public class InstancePerCase : LifecycleBehavior
    {
        readonly LifecycleBehavior inner;

        public InstancePerCase(LifecycleBehavior inner)
        {
            this.inner = inner;
        }

        public void Execute(Type fixtureClass, Case[] cases, Convention convention)
        {
            foreach (var @case in cases)
                inner.Execute(fixtureClass, new[] { @case }, convention);
        }
    }

    public class InstantiateAndExecuteCases : LifecycleBehavior
    {
        readonly InstanceBehavior inner;

        public InstantiateAndExecuteCases(InstanceBehavior inner)
        {
            this.inner = inner;
        }

        public void Execute(Type fixtureClass, Case[] cases, Convention convention)
        {
            object instance;
            var constructionExceptions = new ExceptionList();
            if (!TryConstruct(fixtureClass, constructionExceptions, out instance))
            {
                foreach (var @case in cases)
                    @case.Exceptions.Add(constructionExceptions);
            }
            else
            {
                inner.Execute(fixtureClass, instance, cases, convention);

                var disposalExceptions = Dispose(instance);
                if (disposalExceptions.Any())
                {
                    foreach (var @case in cases)
                        @case.Exceptions.Add(disposalExceptions);
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

        public void Execute(Type testClass, object instance, Case[] cases, Convention convention)
        {
            var instanceSetUpExceptions = setUp(testClass, instance);
            if (instanceSetUpExceptions.Any())
            {
                foreach (var @case in cases)
                    @case.Exceptions.Add(instanceSetUpExceptions);
            }
            else
            {
                inner.Execute(testClass, instance, cases, convention);

                var instanceTearDownExceptions = tearDown(testClass, instance);
                if (instanceTearDownExceptions.Any())
                {
                    foreach (var @case in cases)
                        @case.Exceptions.Add(instanceTearDownExceptions);
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

        public void Execute(Type fixtureClass, Case[] cases, Convention convention)
        {
            var classSetUpExceptions = setUp(fixtureClass);
            if (classSetUpExceptions.Any())
            {
                foreach (var @case in cases)
                    @case.Exceptions.Add(classSetUpExceptions);
            }
            else
            {
                inner.Execute(fixtureClass, cases, convention);

                var classTearDownExceptions = tearDown(fixtureClass);
                foreach (var @case in cases)
                    @case.Exceptions.Add(classTearDownExceptions);
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