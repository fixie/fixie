using System;
using Fixie.Conventions;

namespace Fixie
{
    public class TestClass
    {
        public TestClass(Convention convention, Type type, CaseExecution[] caseExecutions)
        {
            Convention = convention;
            Type = type;
            CaseExecutions = caseExecutions;
        }

        public Convention Convention { get; private set; }
        public Type Type { get; private set; }
        public CaseExecution[] CaseExecutions { get; private set; }
    }
}