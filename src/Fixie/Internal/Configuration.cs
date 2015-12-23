using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fixie.Internal
{
    class Configuration
    {
        readonly List<Func<Type, bool>> testClassConditions;
        readonly List<Func<MethodInfo, bool>> testMethodConditions;
        readonly List<Func<ParameterSource>> parameterSources;
        readonly List<Func<ClassBehavior>> customClassBehaviors;
        readonly List<Func<FixtureBehavior>> customFixtureBehaviors;
        readonly List<Func<CaseBehavior>> customCaseBehaviors;
        readonly List<Type> assertionLibraryTypes;
        readonly List<SkipRule> skipRules;

        public Configuration()
        {
            OrderCases = executions => { };
            ConstructionFrequency = ConstructionFrequency.PerCase;
            TestClassFactory = UseDefaultConstructor;

            testClassConditions = new List<Func<Type, bool>>
            {
                ConcreteClasses,
                NonDiscoveryClasses,
                NonCompilerGeneratedClasses
            };

            testMethodConditions = new List<Func<MethodInfo, bool>>
            {
                ExcludeMethodsDefinedOnObject,
                ExcludeDispose
            };

            parameterSources = new List<Func<ParameterSource>>();
            customClassBehaviors = new List<Func<ClassBehavior>>();
            customFixtureBehaviors = new List<Func<FixtureBehavior>>();
            customCaseBehaviors = new List<Func<CaseBehavior>>();
            assertionLibraryTypes = new List<Type>();
            skipRules = new List<SkipRule>();
        }

        public Action<Case[]> OrderCases { get; set; }
        public ConstructionFrequency ConstructionFrequency { get; set; }
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

        static bool ConcreteClasses(Type type)
        {
            return type.IsClass && !type.IsAbstract;
        }

        static bool NonDiscoveryClasses(Type type)
        {
            return !type.IsSubclassOf(typeof(Convention));
        }

        static bool NonCompilerGeneratedClasses(Type type)
        {
            return !type.Has<CompilerGeneratedAttribute>();
        }

        static bool ExcludeMethodsDefinedOnObject(MethodInfo method)
        {
            return method.DeclaringType != typeof(object);
        }

        static bool ExcludeDispose(MethodInfo method)
        {
            return !method.IsDispose();
        }

        public void AddTestClassCondition(Func<Type, bool> testClassCondition)
        {
            testClassConditions.Add(testClassCondition);
        }

        public void AddTestMethodCondition(Func<MethodInfo, bool> testMethodCondition)
        {
            testMethodConditions.Add(testMethodCondition);
        }

        public void AddParameterSource(Func<ParameterSource> getParameterSource)
        {
            parameterSources.Add(getParameterSource);
        }

        public void WrapClasses(Func<ClassBehavior> getBehavior)
        {
            customClassBehaviors.Insert(0, getBehavior);
        }

        public void WrapFixtures(Func<FixtureBehavior> getBehavior)
        {
            customFixtureBehaviors.Insert(0, getBehavior);
        }

        public void WrapCases(Func<CaseBehavior> getBehavior)
        {
            customCaseBehaviors.Insert(0, getBehavior);
        }

        public void AddAssertionLibraryType(Type libraryInfrastructureType)
        {
            assertionLibraryTypes.Add(libraryInfrastructureType);
        }

        public void AddSkipRule(SkipRule skipRule)
        {
            skipRules.Add(skipRule);
        }

        public IReadOnlyList<Func<Type, bool>> TestClassConditions => testClassConditions;
        public IReadOnlyList<Func<MethodInfo, bool>> TestMethodConditions => testMethodConditions;
        public IReadOnlyList<Func<ParameterSource>> ParameterSources => parameterSources;
        public IReadOnlyList<Func<ClassBehavior>> CustomClassBehaviors => customClassBehaviors;
        public IReadOnlyList<Func<FixtureBehavior>> CustomFixtureBehaviors => customFixtureBehaviors;
        public IReadOnlyList<Func<CaseBehavior>> CustomCaseBehaviors => customCaseBehaviors;
        public IReadOnlyList<Type> AssertionLibraryTypes => assertionLibraryTypes;
        public IReadOnlyList<SkipRule> SkipRules => skipRules;
    }
}