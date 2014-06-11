using System;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie
{
    public class ExecutionPlan
    {
        readonly BehaviorChain<ClassExecution> classBehaviorChain;
        readonly BehaviorChain<InstanceExecution> instanceBehaviorChain;
        readonly BehaviorChain<CaseExecution> caseBehaviorChain;

        public ExecutionPlan(ConfigModel config)
        {
            classBehaviorChain = BuildClassBehaviorChain(config);
            instanceBehaviorChain = BuildInstanceBehaviorChain(config);
            caseBehaviorChain = BuildCaseBehaviorChain(config);
        }

        public void Execute(ClassExecution classExecution)
        {
            classBehaviorChain.Execute(classExecution);
        }

        public void Execute(InstanceExecution instanceExecution)
        {
            instanceBehaviorChain.Execute(instanceExecution);
        }

        public void Execute(CaseExecution caseExecution)
        {
            caseBehaviorChain.Execute(caseExecution);
        }

        BehaviorChain<ClassExecution> BuildClassBehaviorChain(ConfigModel config)
        {
            var chain = new BehaviorChain<ClassExecution>();

            foreach (var customBehavior in config.CustomClassBehaviors)
                chain.Add((ClassBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(GetInnermostBehavior(config));

            return chain;
        }

        BehaviorChain<InstanceExecution> BuildInstanceBehaviorChain(ConfigModel config)
        {
            var chain = new BehaviorChain<InstanceExecution>();

            foreach (var customBehavior in config.CustomInstanceBehaviors)
                chain.Add((InstanceBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(new ExecuteCases());

            return chain;
        }

        BehaviorChain<CaseExecution> BuildCaseBehaviorChain(ConfigModel config)
        {
            var chain = new BehaviorChain<CaseExecution>();

            foreach (var customBehavior in config.CustomCaseBehaviors)
                chain.Add((CaseBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(new Invoke());

            return chain;
        }

        ClassBehavior GetInnermostBehavior(ConfigModel config)
        {
            if (config.ConstructionFrequency == ConstructionFrequency.PerCase)
                return new CreateInstancePerCase(config.Factory);

            return new CreateInstancePerClass(config.Factory);
        }
    }
}