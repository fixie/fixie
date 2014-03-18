using System;

namespace Fixie
{
    public class TestClass
    {
        public TestClass(Type type)
        {
            Type = type;
        }

        public Type Type { get; private set; }
    }
}