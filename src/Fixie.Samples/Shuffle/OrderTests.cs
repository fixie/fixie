using System;
using System.Text;

namespace Fixie.Samples.Shuffle
{
    public class OrderTests : IDisposable
    {
        readonly StringBuilder log;

        public OrderTests()
        {
            log = new StringBuilder();
        }

        public void TestA() { log.WhereAmI(); }
        public void TestB() { log.WhereAmI(); }
        public void TestC() { log.WhereAmI(); }
        public void TestD() { log.WhereAmI(); }
        public void TestE() { log.WhereAmI(); }
        public void TestF() { log.WhereAmI(); }
        public void TestG() { log.WhereAmI(); }
        public void TestH() { log.WhereAmI(); }
        public void TestI() { log.WhereAmI(); }

        public void Dispose()
        {
            log.ShouldHaveLines(
                "TestI",
                "TestB",
                "TestD",
                "TestA",
                "TestC",
                "TestF",
                "TestE",
                "TestH",
                "TestG");
        }
    }
}