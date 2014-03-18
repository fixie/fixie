using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public interface TypeBehavior
    {
        void Execute(TestClass testClass, Convention convention, CaseExecution[] caseExecutions);
    }
}