namespace Fixie
{
    public interface Case
    {
        string Name { get; }
        CaseResult Execute(Listener listener);
    }
}