namespace Fixie.Internal
{
    public class CaseStarted : Message
    {
        public CaseStarted(Case @case)
        {
            Test = new Test(@case.Method);
            Name = @case.Name;
        }

        public Test Test { get; }
        public string Name { get; }
    }
}