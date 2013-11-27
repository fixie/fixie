namespace Fixie.Behaviors
{
    public class Invoke : CaseBehavior
    {
        public void Execute(CaseExecution caseExecution, object instance)
        {
            caseExecution.Case.Execute(instance, caseExecution);
        }
    }
}