namespace Fixie
{
    public class SkipResult
    {
        public SkipResult(Case @case, string reason)
        {
            Case = @case;
            Reason = reason;
        }

        public Case Case { get; private set; }
        public string Reason { get; private set; }
    }
}