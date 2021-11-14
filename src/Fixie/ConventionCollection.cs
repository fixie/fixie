namespace Fixie;

using System.Collections.Generic;
using Internal;

public class ConventionCollection
{
    internal List<Convention> Items { get; }

    internal ConventionCollection() => Items = new List<Convention>();

    public void Add(IDiscovery discovery, IExecution execution)
        => Items.Add(new Convention(discovery, execution));

    public void Add<TDiscovery, TExecution>()
        where TDiscovery : IDiscovery, new()
        where TExecution : IExecution, new()
        => Add(new TDiscovery(), new TExecution());
}