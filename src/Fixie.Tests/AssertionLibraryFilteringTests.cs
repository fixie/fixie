using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Listeners;

namespace Fixie.Tests
{
    public class AssertionLibraryFilteringTests
    {
        public void ShouldNotAffectOutputByDefault()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new ConsoleListener();

                typeof(SampleTestClass).Run(listener, SelfTestConvention.Build());

                console
                    .Output.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(CleanBrittleValues)
                    .ShouldEqual(
                        "------ Testing Assembly Fixie.Tests.dll ------",
                        "",
                        "Test 'Fixie.Tests.AssertionLibraryFilteringTests+SampleTestClass.DivideByZero' failed: System.DivideByZeroException",
                        "Attempted to divide by zero.",
                        "   at Fixie.Tests.AssertionLibraryFilteringTests.SampleTestClass.DivideByZero() in " + PathToThisFile() + ":line #",
                        "",
                        "Test 'Fixie.Tests.AssertionLibraryFilteringTests+SampleTestClass.FailedAssertion' failed: Fixie.Tests.SampleAssertionLibrary.AssertionException",
                        "Expected 1, but was 0.",
                        "   at Fixie.Tests.SampleAssertionLibrary.SampleAssert.AreEqual(Int32 expected, Int32 actual) in " + PathToThisFile() + ":line #",
                        "   at Fixie.Tests.AssertionLibraryFilteringTests.SampleTestClass.FailedAssertion() in " + PathToThisFile() + ":line #",
                        "",
                        "0 passed, 2 failed, took 1.23 seconds (Fixie 1.2.3.4).",
                        "",
                        "");
            }
        }

        public void ShouldFilterAssertionLibraryImplementationDetailsWhenLibraryTypesAreSpecified()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new ConsoleListener();

                var convention = SelfTestConvention.Build();

                convention
                    .HideExceptionDetails
                    .For<SampleAssertionLibrary.AssertionException>()
                    .For(typeof(SampleAssertionLibrary.SampleAssert));

                typeof(SampleTestClass).Run(listener, convention);

                console
                    .Output.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(CleanBrittleValues)
                    .ShouldEqual(
                        "------ Testing Assembly Fixie.Tests.dll ------",
                        "",
                        "Test 'Fixie.Tests.AssertionLibraryFilteringTests+SampleTestClass.DivideByZero' failed: System.DivideByZeroException",
                        "Attempted to divide by zero.",
                        "   at Fixie.Tests.AssertionLibraryFilteringTests.SampleTestClass.DivideByZero() in " + PathToThisFile() + ":line #",
                        "",
                        "Test 'Fixie.Tests.AssertionLibraryFilteringTests+SampleTestClass.FailedAssertion' failed: ",
                        "Expected 1, but was 0.",
                        "   at Fixie.Tests.AssertionLibraryFilteringTests.SampleTestClass.FailedAssertion() in " + PathToThisFile() + ":line #",
                        "",
                        "0 passed, 2 failed, took 1.23 seconds (Fixie 1.2.3.4).",
                        "",
                        "");
            }
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by fixie version.
            var cleaned = Regex.Replace(actualRawContent, @"\(Fixie \d+\.\d+\.\d+\.\d+\)", @"(Fixie 1.2.3.4)");

            //Avoid brittle assertion introduced by test duration.
            cleaned = Regex.Replace(cleaned, @"took [\d\.]+ seconds", @"took 1.23 seconds");

            //Avoid brittle assertion introduced by stack trace line numbers.
            cleaned = Regex.Replace(cleaned, @":line \d+", ":line #");

            return cleaned;
        }

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }

        class SampleTestClass
        {
            public void DivideByZero()
            {
                int x = 0;
                x = x / x;
            }

            public void FailedAssertion()
            {
                SampleAssertionLibrary.SampleAssert.AreEqual(1, 0);
            }
        }
    }

    namespace SampleAssertionLibrary
    {
        public static class SampleAssert
        {
            public static void AreEqual(int expected, int actual)
            {
                if (expected != actual)
                    throw new AssertionException(string.Format("Expected {0}, but was {1}.", expected, actual));
            }
        }

        public class AssertionException : Exception
        {
            public AssertionException(string message)
                : base(message) { }
        }
    }
}