namespace Fixie
{
    public interface Fixture
    {
        string Name { get; }
        void Execute(Listener listener);
    }
}