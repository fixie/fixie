using System;
using System.Globalization;
using System.Threading;
using Fixie.Tests.ConsoleRunner;
using Should;

namespace Fixie.Tests.MetaTests
{
    public class ConsoleListenerTestsTests : IDisposable
    {
        readonly CultureInfo originalCulture;
        readonly Thread currentThread;

        public ConsoleListenerTestsTests()
        {
            currentThread = Thread.CurrentThread;
            originalCulture = currentThread.CurrentCulture;
        }

        public void CleanBrittleValuesShouldCleanTimeOnEnglishLocale()
        {
            currentThread.CurrentCulture = new CultureInfo("en-US");

            var cleaned = ConsoleListenerTests.CleanBrittleValues("took 12.96 seconds");

            cleaned.ShouldEqual("took 1.23 seconds");
        }

        public void CleanBrittleValuesShouldCleanTimeOnGermanLocale()
        {
            currentThread.CurrentCulture = new CultureInfo("de-AT");

            var cleaned = ConsoleListenerTests.CleanBrittleValues("took 12,96 seconds");

            cleaned.ShouldEqual("took 1.23 seconds");
        }

        public void Dispose()
        {
            currentThread.CurrentCulture = originalCulture;
        }
    }
}