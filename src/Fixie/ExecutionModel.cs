using System;
using System.Linq;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie
{
    public class ExecutionModel
    {
        readonly BehaviorChain<ClassExecution> classBehaviorChain;
        readonly BehaviorChain<InstanceExecution> instanceBehaviorChain;
        readonly BehaviorChain<CaseExecution> caseBehaviorChain;
        readonly AssertionLibraryFilter assertionLibraryFilter;

        public ExecutionModel(ConfigModel config)
        {
            classBehaviorChain = BuildClassBehaviorChain(config);
            instanceBehaviorChain = BuildInstanceBehaviorChain(config);
            caseBehaviorChain = BuildCaseBehaviorChain(config);
            assertionLibraryFilter = new AssertionLibraryFilter(config.AssertionLibraryTypes);
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

        public AssertionLibraryFilter AssertionLibraryFilter
        {
            get { return assertionLibraryFilter; }
        }

        static BehaviorChain<ClassExecution> BuildClassBehaviorChain(ConfigModel config)
        {
            var chain = config.CustomClassBehaviors
                .Select(customBehavior => (ClassBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(GetInnermostBehavior(config));

            return new BehaviorChain<ClassExecution>(chain);
        }

        static BehaviorChain<InstanceExecution> BuildInstanceBehaviorChain(ConfigModel config)
        {
            var chain = config.CustomInstanceBehaviors
                .Select(customBehavior => (InstanceBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(new ExecuteCases());

            return new BehaviorChain<InstanceExecution>(chain);
        }

        static BehaviorChain<CaseExecution> BuildCaseBehaviorChain(ConfigModel config)
        {
            var chain = config.CustomCaseBehaviors
                .Select(customBehavior => (CaseBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(new Invoke());

            return new BehaviorChain<CaseExecution>(chain);
        }

        static ClassBehavior GetInnermostBehavior(ConfigModel config)
        {
            if (config.ConstructionFrequency == ConstructionFrequency.PerCase)
                return new CreateInstancePerCase(config.Factory);

            return new CreateInstancePerClass(config.Factory);
        }
    }
}