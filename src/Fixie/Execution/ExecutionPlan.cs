using System;
using System.Linq;
using Fixie.Conventions;
using Fixie.Execution.Behaviors;

namespace Fixie.Execution
{
    public class ExecutionPlan
    {
        readonly BehaviorChain<Class> classBehaviors;

        public ExecutionPlan(Configuration config)
        {
            classBehaviors =
                BuildClassBehaviorChain(config,
                    BuildInstanceBehaviorChain(config,
                        BuildCaseBehaviorChain(config)));
        }

        public void ExecuteClassBehaviors(Class @class)
        {
            classBehaviors.Execute(@class);
        }

        static BehaviorChain<Class> BuildClassBehaviorChain(Configuration config, BehaviorChain<Fixture> instanceBehaviors)
        {
            var chain = config.CustomClassBehaviors
                .Select(customBehavior => (ClassBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(GetInnermostBehavior(config, instanceBehaviors));

            return new BehaviorChain<Class>(chain);
        }

        static BehaviorChain<Fixture> BuildInstanceBehaviorChain(Configuration config, BehaviorChain<Case> caseBehaviors)
        {
            var chain = config.CustomInstanceBehaviors
                .Select(customBehavior => (FixtureBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(new ExecuteCases(caseBehaviors));

            return new BehaviorChain<Fixture>(chain);
        }

        static BehaviorChain<Case> BuildCaseBehaviorChain(Configuration config)
        {
            var chain = config.CustomCaseBehaviors
                .Select(customBehavior => (CaseBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(new InvokeMethod());

            return new BehaviorChain<Case>(chain);
        }

        static ClassBehavior GetInnermostBehavior(Configuration config, BehaviorChain<Fixture> instanceBehaviors)
        {
            if (config.ConstructionFrequency == ConstructionFrequency.PerCase)
                return new CreateInstancePerCase(config.TestClassFactory, instanceBehaviors);

            return new CreateInstancePerClass(config.TestClassFactory, instanceBehaviors);
        }
    }
}