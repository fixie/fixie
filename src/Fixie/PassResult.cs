namespace Fixie
{
    public class PassResult
    {
        public PassResult(CaseResult result)
        {
            Case = result.Case;
            Output = result.Output;
        }

        public Case Case { get; private set; }
        public string Output { get; private set; }
    }
}