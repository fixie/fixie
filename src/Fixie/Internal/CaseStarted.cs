namespace Fixie.Internal
{
    public class CaseStarted : Message
    {
        internal CaseStarted(Case @case)
        {
            Test = @case.Test;
            Name = @case.Name;
        }

        public Test Test { get; }
        public string Name { get; }
    }
}