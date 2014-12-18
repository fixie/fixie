using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fixie.Internal
{
    public class Configuration
    {
        readonly List<Func<Type, bool>> testClassConditions;
        readonly List<Func<MethodInfo, bool>> testMethodConditions;
        readonly List<Func<ParameterSource>> parameterSources;
        readonly List<Func<ClassBehavior>> customClassBehaviors;
        readonly List<Func<FixtureBehavior>> customFixtureBehaviors;
        readonly List<Func<CaseBehavior>> customCaseBehaviors;
        readonly List<Type> assertionLibraryTypes;

        public Configuration()
        {
            OrderCases = executions => { };
            ConstructionFrequency = ConstructionFrequency.PerCase;
            TestClassFactory = UseDefaultConstructor;
            SkipCase = @case => false;
            GetSkipReason = @case => null;

            testClassConditions = new List<Func<Type, bool>>
            {
                ConcreteClasses,
                NonDiscoveryClasses
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
        }

        public Action<Case[]> OrderCases { get; set; }
        public ConstructionFrequency ConstructionFrequency { get; set; }
        public Func<Type, object> TestClassFactory { get; set; }
        public Func<Case, bool> SkipCase { get; set; }
        public Func<Case, string> GetSkipReason { get; set; }

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
            return !type.IsSubclassOf(typeof(Convention)) && !type.IsSubclassOf(typeof(TestAssembly));
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

        public IReadOnlyList<Func<Type, bool>> TestClassConditions { get { return testClassConditions; } }
        public IReadOnlyList<Func<MethodInfo, bool>> TestMethodConditions { get { return testMethodConditions; } }
        public IReadOnlyList<Func<ParameterSource>> ParameterSources { get { return parameterSources; } }
        public IReadOnlyList<Func<ClassBehavior>> CustomClassBehaviors { get { return customClassBehaviors; } }
        public IReadOnlyList<Func<FixtureBehavior>> CustomFixtureBehaviors { get { return customFixtureBehaviors; } }
        public IReadOnlyList<Func<CaseBehavior>> CustomCaseBehaviors { get { return customCaseBehaviors; } }
        public IReadOnlyList<Type> AssertionLibraryTypes { get { return assertionLibraryTypes; } }
    }
}