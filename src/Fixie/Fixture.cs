namespace Fixie
{
    public interface Fixture
    {
        string Name { get; }
        Result Execute(Listener listener);
    }
}