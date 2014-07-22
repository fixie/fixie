namespace Fixie.Tests.Cases
{
    public abstract class CaseTests
    {
        protected CaseTests()
        {
            Listener = new StubListener();
            Convention = SelfTestConvention.Build();
        }

        protected Convention Convention { get; private set; }
        protected StubListener Listener { get; private set; }

        protected void Run<TSampleTestClass>()
        {
            typeof(TSampleTestClass).Run(Listener, Convention);
        }
    }
}