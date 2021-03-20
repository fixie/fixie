namespace Fixie.Reports
{
    using Internal;

    public class CaseStarted : Message
    {
        internal CaseStarted(Case @case)
        {
            Test = @case.Test;
            Name = @case.Name;
        }

        public TestName Test { get; }
        public string Name { get; }
    }
}