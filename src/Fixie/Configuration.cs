namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class Configuration
    {
        readonly List<Func<Type, bool>> testClassConditions;
        readonly List<Func<MethodInfo, bool>> testMethodConditions;
        readonly List<Func<ParameterSource>> parameterSources;
        readonly List<Type> assertionLibraryTypes;
        readonly List<SkipBehavior> skipBehaviors;

        public Configuration()
        {
            OrderCases = executions => { };
            Lifecycle = new DefaultLifecycle();
            TestClassFactory = UseDefaultConstructor;

            testClassConditions = new List<Func<Type, bool>>();
            testMethodConditions = new List<Func<MethodInfo, bool>>();
            parameterSources = new List<Func<ParameterSource>>();
            assertionLibraryTypes = new List<Type>();
            skipBehaviors = new List<SkipBehavior>();
        }

        public Action<Case[]> OrderCases { get; set; }
        public Lifecycle Lifecycle { get; set; }
        public Func<Type, object> TestClassFactory { get; set; }

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

        public void AddParameterSource(Func<ParameterSource> getParameterSource)
            => parameterSources.Add(getParameterSource);

        public void AddAssertionLibraryType(Type libraryInfrastructureType)
            => assertionLibraryTypes.Add(libraryInfrastructureType);

        public void AddSkipBehavior(SkipBehavior skipBehavior)
            => skipBehaviors.Add(skipBehavior);

        public IReadOnlyList<Func<Type, bool>> TestClassConditions => testClassConditions;
        public IReadOnlyList<Func<MethodInfo, bool>> TestMethodConditions => testMethodConditions;
        public IReadOnlyList<Func<ParameterSource>> ParameterSources => parameterSources;
        public IReadOnlyList<Type> AssertionLibraryTypes => assertionLibraryTypes;
        public IReadOnlyList<SkipBehavior> SkipBehaviors => skipBehaviors;
    }
}