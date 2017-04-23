namespace Fixie.Tests.Cases
{
    using System.Linq;
    using static Utility;

    public abstract class CaseTests
    {
        protected CaseTests()
        {
            Listener = new StubListener();
            Convention = SelfTestConvention.Build();
        }

        protected Convention Convention { get; }
        protected StubListener Listener { get; }

        protected void Run<TSampleTestClass>()
            => Utility.Run<TSampleTestClass>(Listener, Convention);

        protected static string[] For<TSampleTestClass>(params string[] entries)
            => entries.Select(x => FullName<TSampleTestClass>() + x).ToArray();
    }
}