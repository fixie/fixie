using System.Collections.Generic;

namespace Fixie
{
    public interface Configuration
    {
        IEnumerable<Fixture> Fixtures { get; }
    }
}