using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;
using Fixie.Discovery;
using Fixie.Results;

namespace Fixie.Execution
{
    public class ClassRunner
    {
        readonly Listener listener;
        readonly ExecutionPlan executionPlan;
        readonly MethodDiscoverer methodDiscoverer;
        readonly AssertionLibraryFilter assertionLibraryFilter;

        readonly Func<MethodInfo, IEnumerable<object[]>> getCaseParameters;
        readonly Func<Case, bool> skipCase;
        readonly Func<Case, string> getSkipReason;
        readonly Action<Case[]> orderCases;

        public ClassRunner(Listener listener, Configuration config)
        {
            this.listener = listener;
            executionPlan = new ExecutionPlan(config);
            methodDiscoverer = new MethodDiscoverer(config);
            assertionLibraryFilter = new AssertionLibraryFilter(config.AssertionLibraryTypes);

            getCaseParameters = config.GetCaseParameters;
            skipCase = config.SkipCase;
            getSkipReason = config.GetSkipReason;
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

                    foreach (var parameters in getCaseParameters(method))
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

            var casesBySkipState = cases.ToLookup(skipCase);
            var casesToSkip = casesBySkipState[true].ToArray();
            var casesToExecute = casesBySkipState[false].ToArray();

            var classResult = new ClassResult(testClass.FullName);

            if (casesToSkip.Any())
            {
                orderCases(casesToSkip);

                foreach (var @case in casesToSkip)
                    classResult.Add(Skip(@case));
            }

            if (casesToExecute.Any())
            {
                orderCases(casesToExecute);

                Run(testClass, casesToExecute);

                foreach (var @case in casesToExecute)
                    classResult.Add(@case.Exceptions.Any() ? Fail(@case) : Pass(@case));
            }

            if (parameterGenerationFailures.Any())
            {
                var casesToFailWithoutRunning = parameterGenerationFailures.ToArray();

                orderCases(casesToFailWithoutRunning);

                foreach (var caseToFailWithoutRunning in casesToFailWithoutRunning)
                    classResult.Add(Fail(caseToFailWithoutRunning));
            }

            return classResult;
        }

        void Run(Type testClass, Case[] casesToExecute)
        {
            executionPlan.ExecuteClassBehaviors(new TestClass(testClass, casesToExecute));
        }

        CaseResult Skip(Case @case)
        {
            var result = new SkipResult(@case, getSkipReason(@case));
            listener.CaseSkipped(result);
            return CaseResult.Skipped(result.Name, result.Reason);
        }

        CaseResult Pass(Case @case)
        {
            var result = new PassResult(@case);
            listener.CasePassed(result);
            return CaseResult.Passed(result.Name, result.Duration);
        }

        CaseResult Fail(Case @case)
        {
            var result = new FailResult(@case, assertionLibraryFilter);
            listener.CaseFailed(result);
            return CaseResult.Failed(result.Name, result.Duration, result.Exceptions);
        }
    }
}