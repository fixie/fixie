using System;
using System.Collections.Generic;
using Fixie.Conventions;

namespace Fixie
{
    public class ClassExecution
    {

        public ClassExecution(Convention convention, Type testClass, CaseExecution[] caseExecutions)
        {
            Convention = convention;
            TestClass = testClass;
            CaseExecutions = caseExecutions;
        }

        public Convention Convention { get; private set; }
        public Type TestClass { get; private set; }
        public IReadOnlyList<CaseExecution> CaseExecutions { get; private set; }

        public void FailCases(Exception exception)
        {
            foreach (var caseExecution in CaseExecutions)
                caseExecution.Fail(exception);
        }
    }
}