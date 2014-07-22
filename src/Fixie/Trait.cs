using System;

namespace Fixie
{
    [Serializable]
    public class Trait
    {
        public Trait(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}