namespace Fixie.Behaviors
{
    public interface CaseBehavior
    {
        void Execute(CaseExecution caseExecution, object instance);
    }
}
