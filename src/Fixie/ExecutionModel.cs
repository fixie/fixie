using System;
using System.Collections.Generic;
using System.Linq;
using Fixie.Behaviors;
using Fixie.Conventions;
using Fixie.Discovery;
using Fixie.Results;

namespace Fixie
{
    public class ExecutionModel
    {
        readonly Listener listener;
        readonly ConfigModel config;
        readonly string conventionName;

        readonly AssertionLibraryFilter assertionLibraryFilter;

        readonly Func<Case, bool> skipCase;
        readonly Func<Case, string> getSkipReason;
        readonly Action<Case[]> orderCases;
        
        public ExecutionModel(Listener listener, Convention convention)
        {
            this.listener = listener;
            config = convention.Config;
            conventionName = convention.GetType().FullName;
            
            assertionLibraryFilter = new AssertionLibraryFilter(config.AssertionLibraryTypes);

            skipCase = config.SkipCase;
            getSkipReason = config.GetSkipReason;
            orderCases = config.OrderCases;
        }

        public ConventionResult Run(Type[] candidateTypes)
        {
            var executionPlan = new ExecutionPlan(config);
            var caseDiscoverer = new CaseDiscoverer(config);
            var conventionResult = new ConventionResult(conventionName);

            foreach (var testClass in caseDiscoverer.TestClasses(candidateTypes))
            {
                var classResult = new ClassResult(testClass.FullName);

                var cases = caseDiscoverer.TestCases(testClass);
                var casesBySkipState = cases.ToLookup(skipCase);
                var casesToSkip = casesBySkipState[true];
                var casesToExecute = casesBySkipState[false].ToArray();
                foreach (var @case in casesToSkip)
                    classResult.Add(Skip(@case));

                if (casesToExecute.Any())
                {
                    orderCases(casesToExecute);

                    var caseExecutions = executionPlan.Execute(testClass, casesToExecute);

                    foreach (var caseExecution in caseExecutions)
                        classResult.Add(caseExecution.Exceptions.Any() ? Fail(caseExecution) : Pass(caseExecution));
                }

                conventionResult.Add(classResult);
            }

            return conventionResult;
        }

        CaseResult Skip(Case @case)
        {
            var result = new SkipResult(@case, getSkipReason(@case));
            listener.CaseSkipped(result);
            return CaseResult.Skipped(result.Case.Name, result.Reason);
        }

        CaseResult Pass(CaseExecution caseExecution)
        {
            var result = new PassResult(caseExecution);
            listener.CasePassed(result);
            return CaseResult.Passed(result.Case.Name, result.Duration);
        }

        CaseResult Fail(CaseExecution caseExecution)
        {
            var result = new FailResult(caseExecution, assertionLibraryFilter);
            listener.CaseFailed(result);
            return CaseResult.Failed(result.Case.Name, result.Duration, result.ExceptionSummary);
        }
    }

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