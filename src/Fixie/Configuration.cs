namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class Configuration
    {
        readonly List<Func<Type, bool>> testClassConditions;
        readonly List<Func<MethodInfo, bool>> testMethodConditions;
        readonly List<ParameterSource> parameterSources;
        readonly List<SkipBehavior> skipBehaviors;

        public Configuration()
        {
            OrderMethods = methods => methods;
            Lifecycle = new DefaultLifecycle();

            testClassConditions = new List<Func<Type, bool>>();
            testMethodConditions = new List<Func<MethodInfo, bool>>();
            parameterSources = new List<ParameterSource>();
            skipBehaviors = new List<SkipBehavior>();
        }

        public Func<IReadOnlyList<MethodInfo>, IReadOnlyList<MethodInfo>> OrderMethods { get; set; }
        public Lifecycle Lifecycle { get; set; }

        static object UseDefaultConstructor(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception.InnerException);
            }
        }

        class DefaultLifecycle : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                runCases(@case =>
                {
                    var instance = UseDefaultConstructor(testClass);

                    @case.Execute(instance);

                    (instance as IDisposable)?.Dispose();
                });
            }
        }

        public void AddTestClassCondition(Func<Type, bool> testClassCondition)
            => testClassConditions.Add(testClassCondition);

        public void AddTestMethodCondition(Func<MethodInfo, bool> testMethodCondition)
            => testMethodConditions.Add(testMethodCondition);

        public void AddParameterSource(ParameterSource parameterSource)
            => parameterSources.Add(parameterSource);

        public void AddSkipBehavior(SkipBehavior skipBehavior)
            => skipBehaviors.Add(skipBehavior);

        public IReadOnlyList<Func<Type, bool>> TestClassConditions => testClassConditions;
        public IReadOnlyList<Func<MethodInfo, bool>> TestMethodConditions => testMethodConditions;
        public IReadOnlyList<ParameterSource> ParameterSources => parameterSources;
        public IReadOnlyList<SkipBehavior> SkipBehaviors => skipBehaviors;
    }
}