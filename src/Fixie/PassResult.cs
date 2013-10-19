namespace Fixie
{
    public class PassResult
    {
        public PassResult(Case @case)
        {
            Case = @case;
        }

        public Case Case { get; private set; }
        public string Output { get { return Case.Output; } }
    }
}