namespace Fixie.Samples.IoC
{
    using System;
    using System.Text;
    using Should;

    public class IoCTests : IDisposable
    {
        readonly IDatabase database;
        readonly IThirdPartyService service;
        readonly StringBuilder log;

        public IoCTests(IDatabase database, IThirdPartyService service)
        {
            this.database = database;
            this.service = service;
            log = new StringBuilder();
            log.WhereAmI();
        }

        public void ShouldReceiveRealDatabase()
        {
            log.WhereAmI();
            database.Query().ShouldEqual("RealDatabase");
        }

        public void ShouldReceiveFakeThirdPartyService()
        {
            log.WhereAmI();
            service.Invoke().ShouldEqual("FakeThirdPartyService");
        }

        public void Dispose()
        {
            log.WhereAmI();
            log.ShouldHaveLines(
                ".ctor",
                "ShouldReceiveFakeThirdPartyService",
                "ShouldReceiveRealDatabase",
                "Dispose");
        }
    }
}