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
        readonly CaseDiscoverer caseDiscoverer;
        readonly AssertionLibraryFilter assertionLibraryFilter;

        readonly Func<MethodInfo, IEnumerable<object[]>> getCaseParameters;
        readonly Func<Case, bool> skipCase;
        readonly Func<Case, string> getSkipReason;
        readonly Action<Case[]> orderCases;

        public ClassRunner(Listener listener, Configuration config)
        {
            this.listener = listener;
            executionPlan = new ExecutionPlan(config);
            caseDiscoverer = new CaseDiscoverer(config);
            assertionLibraryFilter = new AssertionLibraryFilter(config.AssertionLibraryTypes);

            getCaseParameters = config.GetCaseParameters;
            skipCase = config.SkipCase;
            getSkipReason = config.GetSkipReason;
            orderCases = config.OrderCases;
        }

        public ClassResult Run(Type testClass)
        {
            var methods = caseDiscoverer.TestMethods(testClass);
            var cases = new List<Case>();
            foreach (var method in methods)
            {
                var casesForKnownInputParameters = getCaseParameters(method)
                    .Select(parameters => new Case(method, parameters)).ToArray();

                if (casesForKnownInputParameters.Any())
                    cases.AddRange(casesForKnownInputParameters);
                else
                    cases.Add(new Case(method));
            }

            var casesBySkipState = cases.ToLookup(skipCase);
            var casesToSkip = casesBySkipState[true];
            var casesToExecute = casesBySkipState[false].ToArray();



            var classResult = new ClassResult(testClass.FullName);

            foreach (var @case in casesToSkip)
                classResult.Add(Skip(@case));

            if (casesToExecute.Any())
            {
                orderCases(casesToExecute);

                var caseExecutions = Run(testClass, casesToExecute);

                foreach (var caseExecution in caseExecutions)
                    classResult.Add(caseExecution.Exceptions.Any() ? Fail(caseExecution) : Pass(caseExecution));
            }

            return classResult;
        }

        IReadOnlyList<CaseExecution> Run(Type testClass, Case[] casesToExecute)
        {
            var caseExecutions = casesToExecute.Select(@case => new CaseExecution(@case)).ToArray();
            var classExecution = new ClassExecution(testClass, caseExecutions);

            executionPlan.ExecuteClassBehaviors(classExecution);

            return caseExecutions;
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
            return CaseResult.Failed(result.Case.Name, result.Duration, result.Exceptions);
        }
    }
}