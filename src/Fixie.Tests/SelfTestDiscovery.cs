namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

    public static class SelfTestExtensions
    {
        public static IEnumerable<TestMethod> OrderByName(this IEnumerable<TestMethod> tests)
            => tests.OrderBy(x => x.Method.Name, StringComparer.Ordinal);
    }
}