namespace Fixie.Tests
{
    using System;
    using System.Runtime.CompilerServices;
    using Should;

    class SampleTestClass
    {
        public void Pass()
        {
            WhereAmI();
        }

        public void Fail()
        {
            WhereAmI();
            throw new FailureException();
        }

        public void FailByAssertion()
        {
            WhereAmI();
            1.ShouldEqual(2);
        }

        [Skip]
        public void SkipWithoutReason()
        {
            throw new ShouldBeUnreachableException();
        }

        [Skip("Skipped with reason.")]
        public void SkipWithReason()
        {
            throw new ShouldBeUnreachableException();
        }

        static void WhereAmI([CallerMemberName] string member = null)
        {
            Console.Out.WriteLine("Console.Out: " + member);
            Console.Error.WriteLine("Console.Error: " + member);
        }

        public static string FilePath()
        {
            return PathToThisFile();
        }

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }
    }
}