using System;
using System.Collections.Generic;
using Fixie.Conventions;

namespace Fixie
{
    public class TestClass
    {
        readonly CaseExecution[] caseExecutions;

        public TestClass(Convention convention, Type type, CaseExecution[] caseExecutions)
        {
            Convention = convention;
            Type = type;
            this.caseExecutions = caseExecutions;
        }

        public Convention Convention { get; private set; }
        public Type Type { get; private set; }
        public IReadOnlyList<CaseExecution> CaseExecutions { get { return caseExecutions; } }

        public void ShuffleCases(Random random)
        {
            caseExecutions.Shuffle(random);
        }

        public void SortCases(Comparison<Case> comparison)
        {
            Array.Sort(caseExecutions, (caseExecutionA, caseExecutionB) => comparison(caseExecutionA.Case, caseExecutionB.Case));
        }

        public void FailCases(Exception exception)
        {
            foreach (var caseExecution in CaseExecutions)
                caseExecution.Fail(exception);
        }
    }
}