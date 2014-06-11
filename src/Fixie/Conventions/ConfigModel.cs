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

        public ConfigModel()
        {
            OrderCases = executions => { };
            ConstructionFrequency = ConstructionFrequency.PerCase;
            Factory = UseDefaultConstructor;
            SkipCase = @case => false;
            GetSkipReason = @case => null;

            customClassBehaviors = new List<Type>();
            customInstanceBehaviors = new List<Type>();
            customCaseBehaviors = new List<Type>();
        }

        public Action<Case[]> OrderCases { get; set; }
        public ConstructionFrequency ConstructionFrequency { get; set; }
        public Func<Type, object> Factory { get; set; }
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

        public BehaviorChain<ClassExecution> BuildClassBehaviorChain()
        {
            var chain = new BehaviorChain<ClassExecution>();

            foreach (var customBehavior in customClassBehaviors)
                chain.Add((ClassBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(GetInnermostBehavior());

            return chain;
        }

        public BehaviorChain<InstanceExecution> BuildInstanceBehaviorChain()
        {
            var chain = new BehaviorChain<InstanceExecution>();

            foreach (var customBehavior in customInstanceBehaviors)
                chain.Add((InstanceBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(new ExecuteCases());

            return chain;
        }

        public BehaviorChain<CaseExecution> BuildCaseBehaviorChain()
        {
            var chain = new BehaviorChain<CaseExecution>();

            foreach (var customBehavior in customCaseBehaviors)
                chain.Add((CaseBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(new Invoke());

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