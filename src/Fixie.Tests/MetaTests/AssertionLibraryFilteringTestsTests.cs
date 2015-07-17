using System;
using System.Globalization;
using System.Threading;
using Should;

namespace Fixie.Tests.MetaTests
{
    public class AssertionLibraryFilteringTestsTests : IDisposable
    {
        readonly CultureInfo originalCulture;
        readonly Thread currentThread;

        public AssertionLibraryFilteringTestsTests()
        {
            currentThread = Thread.CurrentThread;
            originalCulture = currentThread.CurrentCulture;
        }

        public void CleanBrittleValuesShouldCleanTimeOnEnglishLocale()
        {
            currentThread.CurrentCulture = new CultureInfo("en-US");

            var cleaned = AssertionLibraryFilteringTests.CleanBrittleValues("took 12.96 seconds");

            cleaned.ShouldEqual("took 1.23 seconds");
        }

        public void CleanBrittleValuesShouldCleanTimeOnGermanLocale()
        {
            currentThread.CurrentCulture = new CultureInfo("de-AT");

            var cleaned = AssertionLibraryFilteringTests.CleanBrittleValues("took 12,96 seconds");

            cleaned.ShouldEqual("took 1.23 seconds");
        }

        public void Dispose()
        {
            currentThread.CurrentCulture = originalCulture;
        }
    }
}