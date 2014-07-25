using Fixie.Conventions;
using Fixie.Discovery;
using Fixie.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Execution
{
    public class ClassRunner
    {
        readonly Listener listener;
        readonly ExecutionPlan executionPlan;
        readonly MethodDiscoverer methodDiscoverer;
        readonly ParameterDiscoverer parameterDiscoverer;
        readonly TraitDiscoverer traitDiscoverer;
        readonly TraitFilter traitFilter;
        readonly AssertionLibraryFilter assertionLibraryFilter;

        readonly Func<Case, bool> skipCase;
        readonly Func<Case, string> getSkipReason;
        readonly Action<Case[]> orderCases;

        public ClassRunner(Listener listener, Configuration config, ILookup<string, string> options)
        {
            this.listener = listener;
            executionPlan = new ExecutionPlan(config);
            methodDiscoverer = new MethodDiscoverer(config);
            parameterDiscoverer = new ParameterDiscoverer(config);
            assertionLibraryFilter = new AssertionLibraryFilter(config);
            traitDiscoverer = new TraitDiscoverer(config);
            traitFilter = new TraitFilterParser().GetTraitFilter(options);
            skipCase = config.SkipCase;
            getSkipReason = config.GetSkipReason;
            orderCases = config.OrderCases;
        }

        public ClassResult Run(Type testClass)
        {
            var methods = methodDiscoverer.TestMethods(testClass);

            var cases = new List<Case>();
            var traitDiscoveryFailures = new List<Case>();
            var parameterGenerationFailures = new List<Case>();

            foreach (var method in methods)
            {
                try
                {
                    var traits = Traits(method).ToArray();
                    if (!traitFilter.IsMatch(traits)) continue;

                    try
                    {
                        bool methodHasParameterizedCase = false;

                        foreach (var parameters in Parameters(method))
                        {
                            methodHasParameterizedCase = true;
                            cases.Add(new Case(method, traits, parameters));
                        }

                        if (!methodHasParameterizedCase)
                            cases.Add(new Case(method, traits));
                    }
                    catch (Exception parameterGenerationException)
                    {
                        var @case = new Case(method, traits);
                        @case.Fail(parameterGenerationException);
                        parameterGenerationFailures.Add(@case);
                    }
                }
                catch (Exception traitDiscoveryException)
                {
                    var @case = new Case(method, new Trait[] { });
                    @case.Fail(traitDiscoveryException);
                    traitDiscoveryFailures.Add(@case);
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

            foreach (var failures in new[] { traitDiscoveryFailures, parameterGenerationFailures })
            {
                if (failures.Any())
                {
                    var casesToFailWithoutRunning = failures.ToArray();

                    TryOrderCases(casesToFailWithoutRunning);

                    foreach (var caseToFailWithoutRunning in casesToFailWithoutRunning)
                        classResult.Add(Fail(caseToFailWithoutRunning));
                }
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

        IEnumerable<Trait> Traits(MethodInfo method)
        {
            return traitDiscoverer.GetTraits(method);
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
            return CaseResult.Skipped(result.Name, result.Traits, result.Reason);
        }

        CaseResult Pass(Case @case)
        {
            var result = new PassResult(@case);
            listener.CasePassed(result);
            return CaseResult.Passed(result.Name, result.Traits, result.Duration);
        }

        CaseResult Fail(Case @case)
        {
            var result = new FailResult(@case, assertionLibraryFilter);
            listener.CaseFailed(result);
            return CaseResult.Failed(result.Name, result.Traits, result.Duration, result.Exceptions);
        }
    }
}