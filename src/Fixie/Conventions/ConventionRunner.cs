using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Results;

namespace Fixie.Conventions
{
    public class ConventionRunner
    {
        public ConventionResult Run(Convention convention, Listener listener, params Type[] candidateTypes)
        {
            var config = convention.Config;

            var executionPlan = new ExecutionPlan(config);
            var conventionResult = new ConventionResult(convention.GetType().FullName);

            foreach (var testClass in convention.Classes.Filter(candidateTypes))
            {
                var classResult = new ClassResult(testClass.FullName);

                var methods = convention.Methods.Filter(testClass);

                var cases = methods.SelectMany(method => CasesForMethod(config, method)).ToArray();
                var casesBySkipState = cases.ToLookup(config.SkipCase);
                var casesToSkip = casesBySkipState[true];
                var casesToExecute = casesBySkipState[false].ToArray();
                foreach (var @case in casesToSkip)
                {
                    var skipResult = new SkipResult(@case, config.GetSkipReason(@case));
                    listener.CaseSkipped(skipResult);
                    classResult.Add(CaseResult.Skipped(skipResult.Case.Name, skipResult.Reason));
                }

                if (casesToExecute.Any())
                {
                    config.OrderCases(casesToExecute);

                    var caseExecutions = casesToExecute.Select(@case => new CaseExecution(@case)).ToArray();
                    var classExecution = new ClassExecution(executionPlan, testClass, caseExecutions);
                    executionPlan.Execute(classExecution);

                    foreach (var caseExecution in caseExecutions)
                    {
                        if (caseExecution.Exceptions.Any())
                        {
                            var failResult = new FailResult(caseExecution, convention.HideExceptionDetails);
                            listener.CaseFailed(failResult);
                            classResult.Add(CaseResult.Failed(failResult.Case.Name, failResult.Duration, failResult.ExceptionSummary));
                        }
                        else
                        {
                            var passResult = new PassResult(caseExecution);
                            listener.CasePassed(passResult);
                            classResult.Add(CaseResult.Passed(passResult.Case.Name, passResult.Duration));
                        }
                    }

                }

                conventionResult.Add(classResult);
            }

            return conventionResult;
        }

        static IEnumerable<Case> CasesForMethod(ConfigModel config, MethodInfo method)
        {
            var casesForKnownInputParameters = config.GetCaseParameters(method)
                .Select(parameters => new Case(method, parameters));

            bool any = false;

            foreach (var actualCase in casesForKnownInputParameters)
            {
                any = true;
                yield return actualCase;
            }

            if (!any)
            {
                if (method.GetParameters().Any())
                    yield return new UncallableParameterizedCase(method);
                else
                    yield return new Case(method);
            }
        }
    }
}