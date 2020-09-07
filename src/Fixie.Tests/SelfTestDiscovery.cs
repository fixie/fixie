namespace Fixie.Tests
{
    public class SelfTestDiscovery : Discovery
    {
        public SelfTestDiscovery()
        {
            TestClassConditions.Add(x => x.IsNestedPrivate || x.IsNestedFamily);
            TestClassConditions.Add(x => x.Name.EndsWith("TestClass"));
        }
    }
}