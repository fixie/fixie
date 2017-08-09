namespace Fixie.Execution
{
    public class CasePassed : CaseCompleted
    {
        public CasePassed(Case @case)
            : base(@case)
        {
        }
    }
}