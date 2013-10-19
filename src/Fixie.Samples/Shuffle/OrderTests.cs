using System;
using System.Runtime.CompilerServices;

namespace Fixie.Samples.Shuffle
{
    public class OrderTests
    {
        public void TestA() { WhereAmI(); }
        public void TestB() { WhereAmI(); }
        public void TestC() { WhereAmI(); }
        public void TestD() { WhereAmI(); }
        public void TestE() { WhereAmI(); }
        public void TestF() { WhereAmI(); }
        public void TestG() { WhereAmI(); }
        public void TestH() { WhereAmI(); }
        public void TestI() { WhereAmI(); }

        void WhereAmI([CallerMemberName] string method = null)
        {
            Console.WriteLine(method);
        }
    }
}
