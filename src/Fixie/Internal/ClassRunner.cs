using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Execution;

namespace Fixie.Internal
{
    public class ClassRunner
    {
        readonly Bus bus;
        readonly ExecutionPlan executionPlan;
        readonly MethodDiscoverer methodDiscoverer;
        readonly ParameterDiscoverer parameterDiscoverer;
        readonly AssertionLibraryFilter assertionLibraryFilter;

        readonly IReadOnlyList<SkipRule> skipRules;
        readonly Action<Case[]> orderCases;

        public ClassRunner(Bus bus, Convention convention)
        {
            var config = convention.Config;

            this.bus = bus;
            executionPlan = new ExecutionPlan(convention);
            methodDiscoverer = new MethodDiscoverer(convention);
            parameterDiscoverer = new ParameterDiscoverer(convention);
            assertionLibraryFilter = new AssertionLibraryFilter(convention);

            skipRules = config.SkipRules;
            orderCases = config.OrderCases;
        }

        public void Run(Type testClass)
        {
            var methods = methodDiscoverer.TestMethods(testClass);

            var cases = new List<Case>();
            var parameterGenerationFailures = new List<Case>();

            foreach (var method in methods)
            {
                try
                {
                    bool methodHasParameterizedCase = false;

                    foreach (var parameters in Parameters(method))
                    {
                        methodHasParameterizedCase = true;
                        cases.Add(new Case(method, parameters));
                    }

                    if (!methodHasParameterizedCase)
                        cases.Add(new Case(method));
                }
                catch (Exception parameterGenerationException)
                {
                    var @case = new Case(method);
                    @case.Fail(parameterGenerationException);
                    parameterGenerationFailures.Add(@case);
                }
            }

            var casesToSkipList = new List<KeyValuePair<Case, string>>();
            var casesToExecuteList = new List<Case>();
            foreach (var @case in cases)
            {
                string reason;
                if (SkipCase(@case, out reason))
                    casesToSkipList.Add(new KeyValuePair<Case, string>(@case, reason));
                else
                    casesToExecuteList.Add(@case);
            }

            var casesToSkip = casesToSkipList.Select(x => x.Key).ToArray();
            var casesToExecute = casesToExecuteList.ToArray();

            if (casesToSkip.Any())
            {
                TryOrderCases(casesToSkip);

                foreach (var @case in casesToSkip)
                    Skip(@case, casesToSkipList.Single(x => x.Key == @case).Value);
            }

            if (casesToExecute.Any())
            {
                if (TryOrderCases(casesToExecute))
                    Run(testClass, casesToExecute);

                foreach (var @case in casesToExecute)
                {
                    if (@case.Exceptions.Any())
                        Fail(@case);
                    else
                        Pass(@case);
                }
            }

            if (parameterGenerationFailures.Any())
            {
                var casesToFailWithoutRunning = parameterGenerationFailures.ToArray();

                TryOrderCases(casesToFailWithoutRunning);

                foreach (var caseToFailWithoutRunning in casesToFailWithoutRunning)
                    Fail(caseToFailWithoutRunning);
            }
        }

        bool SkipCase(Case @case, out string reason)
        {
            foreach (var rule in skipRules)
            {
                string ruleReason;
                if (rule.AppliesTo(@case, out ruleReason))
                {
                    reason = ruleReason;
                    return true;
                }
            }

            reason = null;
            return false;
        }

        bool TryOrderCases(Case[] cases)
        {
            try
            {
                orderCases(cases);
            }
            catch (Exception exception)
            {
                foreach (var @case in cases)
                    @case.Fail(exception);

                return false;
            }

            return true;
        }

        IEnumerable<object[]> Parameters(MethodInfo method)
            => parameterDiscoverer.GetParameters(method);

        void Run(Type testClass, Case[] casesToExecute)
            => executionPlan.ExecuteClassBehaviors(new Class(testClass, casesToExecute));

        void Skip(Case @case, string reason)
            => bus.Publish(CaseCompleted.Skipped(@case, reason));

        void Pass(Case @case)
            => bus.Publish(CaseCompleted.Passed(@case));

        void Fail(Case @case)
            => bus.Publish(CaseCompleted.Failed(@case, assertionLibraryFilter));
    }
}