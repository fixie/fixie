using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public class ConfigModel
    {
        readonly List<Type> customClassBehaviors;

        public ConfigModel()
        {
            OrderCases = executions => { };
            ConstructionFrequency = ConstructionFrequency.PerCase;
            Factory = UseDefaultConstructor;

            customClassBehaviors = new List<Type>();
        }

        public Action<Case[]> OrderCases { get; set; }

        public ConstructionFrequency ConstructionFrequency { get; set; }

        public Func<Type, object> Factory { get; set; }

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

        public BehaviorChain<ClassExecution> BuildClassBehaviorChain()
        {
            var chain = new BehaviorChain<ClassExecution>();

            foreach (var customBehavior in customClassBehaviors)
                chain.Add((ClassBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(GetInnermostBehavior());

            return chain;
        }

        ClassBehavior GetInnermostBehavior()
        {
            if (ConstructionFrequency == ConstructionFrequency.PerCase)
                return new CreateInstancePerCase(Factory);

            return new CreateInstancePerClass(Factory);
        }
    }
}