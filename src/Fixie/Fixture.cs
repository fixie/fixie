using System;

namespace Fixie
{
    public class Fixture
    {
        public Fixture(Type type, object instance, Case[] cases)
        {
            Type = type;
            Instance = instance;
            Cases = cases;
        }

        public Type Type { get; private set; }
        public object Instance { get; private set; }
        public Case[] Cases { get; private set; }
    }
}