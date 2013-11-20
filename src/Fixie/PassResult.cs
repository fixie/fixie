using System;

namespace Fixie
{
    public class PassResult
    {
        public PassResult(CaseResult result)
        {
            Case = result.Case;
            Output = result.Output;
            Duration = result.Duration;
        }

        public Case Case { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
    }
}