namespace Fixie.Execution
{
    public interface IListenerFactory
    {
        Listener Create(Options options);
    }
}