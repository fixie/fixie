namespace Fixie.Tests
{
    using System;

    public class SelfTestDiscovery : Discovery
    {
        public SelfTestDiscovery()
        {
            Classes
                .Where(x => x.IsNestedPrivate || x.IsNestedFamily)
                .Where(x => x.Name.EndsWith("TestClass"));

            Methods
                .OrderBy(x => x.Name, StringComparer.Ordinal);
        }
    }
}