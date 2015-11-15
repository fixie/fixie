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

        public ClassRunner(Bus bus, Configuration config)
        {
            this.bus = bus;
            executionPlan = new ExecutionPlan(config);
            methodDiscoverer = new MethodDiscoverer(config);
            parameterDiscoverer = new ParameterDiscoverer(config);
            assertionLibraryFilter = new AssertionLibraryFilter(config);

            skipRules = config.SkipRules;
            orderCases = config.OrderCases;
        }

        public ClassResult Run(Type testClass)
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

            var classResult = new ClassResult(testClass.FullName);

            if (casesToSkip.Any())
            {
                TryOrderCases(casesToSkip);

                foreach (var @case in casesToSkip)
                    classResult.Add(Skip(@case, casesToSkipList.Single(x => x.Key == @case).Value));
            }

            if (casesToExecute.Any())
            {
                if (TryOrderCases(casesToExecute))
                    Run(testClass, casesToExecute);

                foreach (var @case in casesToExecute)
                    classResult.Add(@case.Exceptions.Any() ? Fail(@case) : Pass(@case));
            }

            if (parameterGenerationFailures.Any())
            {
                var casesToFailWithoutRunning = parameterGenerationFailures.ToArray();

                TryOrderCases(casesToFailWithoutRunning);

                foreach (var caseToFailWithoutRunning in casesToFailWithoutRunning)
                    classResult.Add(Fail(caseToFailWithoutRunning));
            }

            return classResult;
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
        {
            return parameterDiscoverer.GetParameters(method);
        }

        void Run(Type testClass, Case[] casesToExecute)
        {
            executionPlan.ExecuteClassBehaviors(new Class(testClass, casesToExecute));
        }

        CaseResult Skip(Case @case, string reason)
        {
            var result = new SkipResult(@case, reason);
            bus.Publish(result);
            return new CaseResult(result);
        }

        CaseResult Pass(Case @case)
        {
            var result = new PassResult(@case);
            bus.Publish(result);
            return new CaseResult(result);
        }

        CaseResult Fail(Case @case)
        {
            var result = new FailResult(@case, assertionLibraryFilter);
            bus.Publish(result);
            return new CaseResult(result);
        }
    }
}