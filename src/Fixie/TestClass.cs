using System;
using Fixie.Conventions;

namespace Fixie
{
    public class TestClass
    {
        public TestClass(Convention convention, Type type)
        {
            Convention = convention;
            Type = type;
        }

        public Convention Convention { get; private set; }
        public Type Type { get; private set; }
    }
}