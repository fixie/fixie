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
        readonly ParameterDiscoverer parameterDiscoverer;
        readonly AssertionLibraryFilter assertionLibraryFilter;

        readonly Func<Case, bool> skipCase;
        readonly Func<Case, string> getSkipReason;
        readonly Action<Case[]> orderCases;

        public ClassRunner(Listener listener, Configuration config)
        {
            this.listener = listener;
            executionPlan = new ExecutionPlan(config);
            methodDiscoverer = new MethodDiscoverer(config);
            parameterDiscoverer = new ParameterDiscoverer(config);
            assertionLibraryFilter = new AssertionLibraryFilter(config);

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

            var casesBySkipState = cases.ToLookup(SkipCase);
            var casesToSkip = casesBySkipState[true].ToArray();
            var casesToExecute = casesBySkipState[false].ToArray();

            var classResult = new ClassResult(testClass.FullName);

            if (casesToSkip.Any())
            {
                TryOrderCases(casesToSkip);

                foreach (var @case in casesToSkip)
                    classResult.Add(Skip(@case));
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

        bool SkipCase(Case @case)
        {
            try
            {
                return skipCase(@case);
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to run a custom case-skipping predicate. " +
                    "Check the inner exception for more details.", exception);
            }
        }

        string GetSkipReason(Case @case)
        {
            try
            {
                return getSkipReason(@case);
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to get a custom case-skipped reason. " +
                    "Check the inner exception for more details.", exception);
            }
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

        CaseResult Skip(Case @case)
        {
            var result = new SkipResult(@case, GetSkipReason(@case));
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