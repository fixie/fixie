namespace Fixie.Tests
{
    using System;

    public class SelfTestConvention : Convention
    {
        public SelfTestConvention()
        {
            Classes
                .Where(x => x.IsNestedPrivate)
                .Where(x => x.Name.EndsWith("TestClass"));

            Methods
                .OrderBy(x => x.Name, StringComparer.Ordinal);
        }
    }

    public class SelfTestDiscovery : Discovery
    {
        public SelfTestDiscovery()
        {
            Classes
                .Where(x => x.IsNestedPrivate)
                .Where(x => x.Name.EndsWith("TestClass"));

            Methods
                .OrderBy(x => x.Name, StringComparer.Ordinal);
        }
    }
}