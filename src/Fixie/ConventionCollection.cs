using Fixie.Internal;

namespace Fixie;

public class ConventionCollection
{
    internal List<Convention> Items { get; }

    internal ConventionCollection() => Items = [];

    public void Add(IDiscovery discovery, IExecution execution)
        => Items.Add(new Convention(discovery, execution));

    public void Add<TDiscovery, TExecution>()
        where TDiscovery : IDiscovery, new()
        where TExecution : IExecution, new()
        => Add(new TDiscovery(), new TExecution());
}