using System;
using System.Linq;
using Fixie.Results;

namespace Fixie.Conventions
{
    public class ConventionRunner
    {
        public ConventionResult Run(Convention convention, Listener listener, params Type[] candidateTypes)
        {
            var config = convention.Config;

            var discoveryModel = new DiscoveryModel(config);
            var executionModel = new ExecutionModel(config);
            var conventionResult = new ConventionResult(convention.GetType().FullName);

            foreach (var testClass in discoveryModel.TestClasses(candidateTypes))
            {
                var classResult = new ClassResult(testClass.FullName);

                var cases = discoveryModel.TestCases(testClass);
                var casesBySkipState = cases.ToLookup(executionModel.SkipCase);
                var casesToSkip = casesBySkipState[true];
                var casesToExecute = casesBySkipState[false].ToArray();
                foreach (var @case in casesToSkip)
                {
                    var skipResult = new SkipResult(@case, executionModel.GetSkipReason(@case));
                    listener.CaseSkipped(skipResult);
                    classResult.Add(CaseResult.Skipped(skipResult.Case.Name, skipResult.Reason));
                }

                if (casesToExecute.Any())
                {
                    executionModel.OrderCases(casesToExecute);

                    var caseExecutions = executionModel.Execute(testClass, casesToExecute);

                    foreach (var caseExecution in caseExecutions)
                    {
                        if (caseExecution.Exceptions.Any())
                        {
                            var failResult = new FailResult(caseExecution, executionModel.AssertionLibraryFilter);
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
    }
}