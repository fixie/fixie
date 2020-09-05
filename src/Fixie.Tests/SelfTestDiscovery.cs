namespace Fixie.Tests
{
    public class SelfTestDiscovery : Discovery
    {
        public SelfTestDiscovery()
        {
            Classes
                .Where(x => x.IsNestedPrivate || x.IsNestedFamily)
                .Where(x => x.Name.EndsWith("TestClass"));
        }
    }
}