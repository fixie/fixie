using System;

namespace Fixie
{
    public class Fixture
    {
        public Fixture(Type type, object instance)
        {
            Type = type;
            Instance = instance;
        }

        public Type Type { get; private set; }
        public object Instance { get; private set; }
    }
}