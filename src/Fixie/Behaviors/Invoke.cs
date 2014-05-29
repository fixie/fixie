using System;

namespace Fixie.Behaviors
{
    public class Invoke : CaseBehavior
    {
        public void Execute(CaseExecution caseExecution, Action next)
        {
            caseExecution.Case.Execute(caseExecution.Instance, caseExecution);
        }
    }
}