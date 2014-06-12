using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public class ConfigModel
    {
        readonly List<Type> customClassBehaviors;
        readonly List<Type> customInstanceBehaviors;
        readonly List<Type> customCaseBehaviors;
        readonly List<Type> assertionLibraryTypes;

        public ConfigModel()
        {
            OrderCases = executions => { };
            ConstructionFrequency = ConstructionFrequency.PerCase;
            Factory = UseDefaultConstructor;
            SkipCase = @case => false;
            GetSkipReason = @case => null;
            GetCaseParameters = method => new object[][] { };

            customClassBehaviors = new List<Type>();
            customInstanceBehaviors = new List<Type>();
            customCaseBehaviors = new List<Type>();
            assertionLibraryTypes = new List<Type>();
        }

        public Action<Case[]> OrderCases { get; set; }
        public ConstructionFrequency ConstructionFrequency { get; set; }
        public Func<Type, object> Factory { get; set; }
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

        public void WrapClasses<TClassBehavior>() where TClassBehavior : ClassBehavior
        {
            customClassBehaviors.Insert(0, typeof(TClassBehavior));
        }

        public void WrapInstances<TInstanceBehavior>() where TInstanceBehavior : InstanceBehavior
        {
            customInstanceBehaviors.Insert(0, typeof(TInstanceBehavior));
        }

        public void WrapCases<TCaseBehavior>() where TCaseBehavior : CaseBehavior
        {
            customCaseBehaviors.Insert(0, typeof(TCaseBehavior));
        }

        public void AddAssertionLibraryType(Type libraryInfrastructureType)
        {
            assertionLibraryTypes.Add(libraryInfrastructureType);
        }

        public IReadOnlyList<Type> CustomClassBehaviors { get { return customClassBehaviors; } }
        public IReadOnlyList<Type> CustomInstanceBehaviors { get { return customInstanceBehaviors; } }
        public IReadOnlyList<Type> CustomCaseBehaviors { get { return customCaseBehaviors; } }
        public IReadOnlyList<Type> AssertionLibraryTypes { get { return assertionLibraryTypes; } }
    }
}