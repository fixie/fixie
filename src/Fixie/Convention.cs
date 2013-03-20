using System.Collections.Generic;

namespace Fixie
{
    public interface Convention
    {
        IEnumerable<Fixture> Fixtures { get; }
        Result Execute(Listener listener);
    }
}