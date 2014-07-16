using System;
using System.Collections.Generic;
using System.Linq;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie
{
    public class ExecutionPlan
    {
        readonly ConfigModel config;

        readonly BehaviorChain<ClassExecution> classBehaviorChain;
        readonly BehaviorChain<InstanceExecution> instanceBehaviorChain;
        readonly BehaviorChain<CaseExecution> caseBehaviorChain;

        public ExecutionPlan(ConfigModel config)
        {
            this.config = config;

            classBehaviorChain = BuildClassBehaviorChain();
            instanceBehaviorChain = BuildInstanceBehaviorChain();
            caseBehaviorChain = BuildCaseBehaviorChain();
        }

        public IReadOnlyList<CaseExecution> Execute(Type testClass, Case[] casesToExecute)
        {
            var caseExecutions = casesToExecute.Select(@case => new CaseExecution(@case)).ToArray();
            var classExecution = new ClassExecution(testClass, caseExecutions);

            classBehaviorChain.Execute(classExecution);

            return caseExecutions;
        }

        public void PerformClassLifecycle(Type testClass, IReadOnlyList<CaseExecution> caseExecutionsForThisInstance)
        {
            var instance = config.TestClassFactory(testClass);

            var instanceExecution = new InstanceExecution(testClass, instance, caseExecutionsForThisInstance);
            instanceBehaviorChain.Execute(instanceExecution);

            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        public void ExecuteCaseBehaviors(CaseExecution caseExecution)
        {
            caseBehaviorChain.Execute(caseExecution);
        }

        BehaviorChain<ClassExecution> BuildClassBehaviorChain()
        {
            var chain = config.CustomClassBehaviors
                .Select(customBehavior => (ClassBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(GetInnermostBehavior());

            return new BehaviorChain<ClassExecution>(chain);
        }

        BehaviorChain<InstanceExecution> BuildInstanceBehaviorChain()
        {
            var chain = config.CustomInstanceBehaviors
                .Select(customBehavior => (InstanceBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(new ExecuteCases(this));

            return new BehaviorChain<InstanceExecution>(chain);
        }

        BehaviorChain<CaseExecution> BuildCaseBehaviorChain()
        {
            var chain = config.CustomCaseBehaviors
                .Select(customBehavior => (CaseBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(new InvokeMethod());

            return new BehaviorChain<CaseExecution>(chain);
        }

        ClassBehavior GetInnermostBehavior()
        {
            if (config.ConstructionFrequency == ConstructionFrequency.PerCase)
                return new CreateInstancePerCase(this);

            return new CreateInstancePerClass(this);
        }
    }
}