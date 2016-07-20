using System.Linq;
using Fixie.Internal.Behaviors;

namespace Fixie.Internal
{
    using System.Collections.Generic;

    public class ExecutionPlan
    {
        readonly BehaviorChain<Class> classBehaviors;

        public ExecutionPlan(Convention convention)
        {
            var config = convention.Config;

            classBehaviors =
                BuildClassBehaviorChain(config,
                    BuildFixtureBehaviorChain(config,
                        BuildCaseBehaviorChain(config)));
        }

        public void ExecuteClassBehaviors(Class testClass)
        {
            classBehaviors.Execute(testClass);
        }

        static BehaviorChain<Class> BuildClassBehaviorChain(Configuration config, BehaviorChain<Fixture> fixtureBehaviors)
        {
            var chain = config.CustomClassBehaviors
                .Select(customBehavior => customBehavior())
                .ToList();

            chain.Add(GetInnermostBehavior(config, fixtureBehaviors));

            chain.Insert(0, new TimeClassExecution());

            return new BehaviorChain<Class>(chain);
        }

        static BehaviorChain<Fixture> BuildFixtureBehaviorChain(Configuration config, BehaviorChain<Case> caseBehaviors)
        {
            var chain = config.CustomFixtureBehaviors
                .Select(customBehavior => customBehavior())
                .ToList();

            chain.Add(new ExecuteCases(caseBehaviors));

            return new BehaviorChain<Fixture>(chain);
        }

        static BehaviorChain<Case> BuildCaseBehaviorChain(Configuration config)
        {
            var chain = config.CustomCaseBehaviors
                .Select(customBehavior => customBehavior())
                .ToList();

            chain.Add(new InvokeMethod());

            return new BehaviorChain<Case>(chain);
        }

        static ClassBehavior GetInnermostBehavior(Configuration config, BehaviorChain<Fixture> fixtureBehaviors)
        {
            if (config.ConstructionFrequency == ConstructionFrequency.PerCase)
                return new CreateInstancePerCase(config.TestClassFactory, fixtureBehaviors);

            return new CreateInstancePerClass(config.TestClassFactory, fixtureBehaviors);
        }
    }
}