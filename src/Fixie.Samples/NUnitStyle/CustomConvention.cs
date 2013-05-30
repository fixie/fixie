using System;
using System.Reflection;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.Samples
{
    public delegate ExceptionList ClassAction(Type testClass);
    public delegate ExceptionList InstanceAction(Type testClass, object instance);

    public static class MethodFilterExtensions
    {
        public static ExceptionList InvokeAll(this MethodFilter methodFilter, Type testClass, object instance)
        {
            var invoke = new Invoke();
            var exceptions = new ExceptionList();
            foreach (var method in methodFilter.Filter(testClass))
                invoke.Execute(method, instance, exceptions);
            return exceptions;
        }
    }

    public static class BehaviorExtensions
    {
        public static MethodBehavior SetUpTearDown<TSetUpAttribute, TTearDownAttribute>(this MethodBehavior inner)
            where TSetUpAttribute : Attribute
            where TTearDownAttribute : Attribute
        {
            return new MethodSetUpTearDown(
                new MethodFilter().HasOrInherits<TSetUpAttribute>(),
                inner,
                new MethodFilter().HasOrInherits<TTearDownAttribute>());
        }

        public static InstanceBehavior SetUpTearDown<TSetUpAttribute, TTearDownAttribute>(this InstanceBehavior inner)
            where TSetUpAttribute : Attribute
            where TTearDownAttribute : Attribute
        {
            return new InstanceSetUpTearDown(
                new MethodFilter().HasOrInherits<TSetUpAttribute>(),
                inner,
                new MethodFilter().HasOrInherits<TTearDownAttribute>());
        }
    }

    public class MethodSetUpTearDown : MethodBehavior
    {
        readonly InstanceAction setUp;
        readonly MethodBehavior inner;
        readonly InstanceAction tearDown;

        public MethodSetUpTearDown(MethodFilter setUps, MethodBehavior inner, MethodFilter tearDowns)
        {
            setUp = setUps.InvokeAll;
            this.inner = inner;
            tearDown = tearDowns.InvokeAll;
        }

        public MethodSetUpTearDown(InstanceAction setUp, MethodBehavior inner, InstanceAction tearDown)
        {
            this.setUp = setUp;
            this.inner = inner;
            this.tearDown = tearDown;
        }

        public void Execute(MethodInfo method, object instance, ExceptionList exceptions)
        {
            var setUpExceptions = setUp(method.ReflectedType, instance);
            if (setUpExceptions.Any())
            {
                exceptions.Add(setUpExceptions);
            }
            else
            {
                inner.Execute(method, instance, exceptions);

                var tearDownExceptions = tearDown(method.ReflectedType, instance);
                if (tearDownExceptions.Any())
                {
                    exceptions.Add(tearDownExceptions);
                }
            }
        }
    }

    public class InstanceSetUpTearDown : InstanceBehavior
    {
        readonly InstanceAction setUp;
        readonly InstanceBehavior inner;
        readonly InstanceAction tearDown;

        public InstanceSetUpTearDown(MethodFilter setUps, InstanceBehavior inner, MethodFilter tearDowns)
        {
            setUp = setUps.InvokeAll;
            this.inner = inner;
            tearDown = tearDowns.InvokeAll;
        }

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

    public class ClassSetUpTearDown : TypeBehavior
    {
        readonly ClassAction setUp;
        readonly TypeBehavior inner;
        readonly ClassAction tearDown;

        public ClassSetUpTearDown(ClassAction setUp, TypeBehavior inner, ClassAction tearDown)
        {
            this.setUp = setUp;
            this.inner = inner;
            this.tearDown = tearDown;
        }

        public void Execute(Type fixtureClass, Convention convention, Case[] cases)
        {
            var classSetUpExceptions = setUp(fixtureClass);
            if (classSetUpExceptions.Any())
            {
                foreach (var @case in cases)
                    @case.Exceptions.Add(classSetUpExceptions);
            }
            else
            {
                inner.Execute(fixtureClass, convention, cases);

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
        public CustomConvention()
        {
            Fixtures
                .HasOrInherits<TestFixtureAttribute>();

            Cases
                .HasOrInherits<TestAttribute>();

            FixtureExecutionBehavior = new CreateInstancePerFixture();

            InstanceExecutionBehavior =
                InstanceExecutionBehavior
                    .SetUpTearDown<TestFixtureSetUpAttribute, TestFixtureTearDownAttribute>();

            CaseExecutionBehavior =
                CaseExecutionBehavior
                    .SetUpTearDown<SetUpAttribute, TearDownAttribute>();
        }
    }
}