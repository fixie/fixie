namespace Fixie.Behaviors
{
    public class Invoke : CaseBehavior
    {
        public void Execute(CaseExecution caseExecution)
        {
            caseExecution.Case.Execute(caseExecution.Instance, caseExecution);
        }
    }
}