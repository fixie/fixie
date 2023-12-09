namespace Fixie.Internal;

class Convention
{
    public Convention(IDiscovery discovery, IExecution execution)
    {
        Discovery = discovery;
        Execution = execution;
    }

    public IDiscovery Discovery { get; }
    public IExecution Execution { get; }
}