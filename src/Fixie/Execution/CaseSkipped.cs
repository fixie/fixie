namespace Fixie.Execution
{
    public class CaseSkipped : IMessage
    {
        public CaseSkipped(Case @case, string skipReason)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            SkipReason = skipReason;
        }

        public string Name { get; }
        public MethodGroup MethodGroup { get; }
        public string SkipReason { get; }
    }
}