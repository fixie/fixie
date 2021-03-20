namespace Fixie.Reports
{
    using Internal;

    public class CaseStarted : Message
    {
        internal CaseStarted(Case @case)
        {
            Test = @case.Test.Name;
            Name = @case.Name;
        }

        public string Test { get; }
        public string Name { get; }
    }
}