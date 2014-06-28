using System;
using System.Collections.Generic;
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

        readonly Func<Case, bool> skipCase;
        readonly Func<Case, string> getSkipReason;
        readonly Action<Case[]> orderCases;

        public ExecutionModel(ConfigModel config)
        {
            classBehaviorChain = BuildClassBehaviorChain(config);
            instanceBehaviorChain = BuildInstanceBehaviorChain(config);
            caseBehaviorChain = BuildCaseBehaviorChain(config);
            assertionLibraryFilter = new AssertionLibraryFilter(config.AssertionLibraryTypes);

            skipCase = config.SkipCase;
            getSkipReason = config.GetSkipReason;
            orderCases = config.OrderCases;
        }

        public IReadOnlyList<CaseExecution> Execute(Type testClass, Case[] casesToExecute)
        {
            var caseExecutions = casesToExecute.Select(@case => new CaseExecution(@case)).ToArray();
            var classExecution = new ClassExecution(this, testClass, caseExecutions);

            classBehaviorChain.Execute(classExecution);

            return caseExecutions;
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

        public bool SkipCase(Case @case)
        {
            return skipCase(@case);
        }

        public string GetSkipReason(Case @case)
        {
            return getSkipReason(@case);
        }

        public void OrderCases(Case[] cases)
        {
            orderCases(cases);
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