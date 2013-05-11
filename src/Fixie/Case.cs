using System;
using System.Collections.Generic;

namespace Fixie
{
    public interface Case
    {
        string Name { get; }
        void Execute(Listener listener, List<Exception> exceptions);
    }
}