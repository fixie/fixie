namespace Fixie.Execution
{
    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case, AssertionLibraryFilter filter)
            : base(@case)
        {
            Exception = new CompoundException(@case.Exceptions, filter);
        }

        public CompoundException Exception { get; }
    }
}