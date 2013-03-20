namespace Fixie
{
    public interface Case
    {
        string Name { get; }
        Result Execute(Listener listener);
    }
}