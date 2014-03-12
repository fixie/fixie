using Fixie.Conventions;

namespace Fixie.Tests.Cases
{
    public abstract class CaseTests
    {
        protected CaseTests()
        {
            Listener = new StubListener();
            Convention = new SelfTestConvention();
        }

        protected SelfTestConvention Convention { get; private set; }
        protected StubListener Listener { get; private set; }

        protected void Run<TSampleTestClass>()
        {
            typeof(TSampleTestClass).Run(Listener, Convention);
        }
    }
}