using System.Collections.Generic;

namespace Fixie
{
    public interface Fixture
    {
        string Name { get; }
        IEnumerable<Case> Cases { get; }
    }
}