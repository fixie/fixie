using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fixie.Conventions
{
    public class Configuration
    {
        readonly List<Func<Type, bool>> testClassConditions;
        readonly List<Func<MethodInfo, bool>> testMethodConditions;

        readonly List<Type> customClassBehaviors;
        readonly List<Type> customFixtureBehaviors;
        readonly List<Type> customCaseBehaviors;
        readonly List<Type> assertionLibraryTypes;

        public Configuration()
        {
            OrderCases = executions => { };
            ConstructionFrequency = ConstructionFrequency.PerCase;
            TestClassFactory = UseDefaultConstructor;
            SkipCase = @case => false;
            GetSkipReason = @case => null;
            GetCaseParameters = method => new object[][] { };

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

            customClassBehaviors = new List<Type>();
            customFixtureBehaviors = new List<Type>();
            customCaseBehaviors = new List<Type>();
            assertionLibraryTypes = new List<Type>();
        }

        public Action<Case[]> OrderCases { get; set; }
        public ConstructionFrequency ConstructionFrequency { get; set; }
        public Func<Type, object> TestClassFactory { get; set; }
        public Func<Case, bool> SkipCase { get; set; }
        public Func<Case, string> GetSkipReason { get; set; }
        public Func<MethodInfo, IEnumerable<object[]>> GetCaseParameters { get; set; }

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

        public void WrapClasses<TClassBehavior>() where TClassBehavior : ClassBehavior
        {
            customClassBehaviors.Insert(0, typeof(TClassBehavior));
        }

        public void WrapFixtures<TFixtureBehavior>() where TFixtureBehavior : FixtureBehavior
        {
            customFixtureBehaviors.Insert(0, typeof(TFixtureBehavior));
        }

        public void WrapCases<TCaseBehavior>() where TCaseBehavior : CaseBehavior
        {
            customCaseBehaviors.Insert(0, typeof(TCaseBehavior));
        }

        public void AddAssertionLibraryType(Type libraryInfrastructureType)
        {
            assertionLibraryTypes.Add(libraryInfrastructureType);
        }

        public IReadOnlyList<Func<Type, bool>> TestClassConditions { get { return testClassConditions; } }
        public IReadOnlyList<Func<MethodInfo, bool>> TestMethodConditions { get { return testMethodConditions; } }
        public IReadOnlyList<Type> CustomClassBehaviors { get { return customClassBehaviors; } }
        public IReadOnlyList<Type> CustomFixtureBehaviors { get { return customFixtureBehaviors; } }
        public IReadOnlyList<Type> CustomCaseBehaviors { get { return customCaseBehaviors; } }
        public IReadOnlyList<Type> AssertionLibraryTypes { get { return assertionLibraryTypes; } }
    }
}