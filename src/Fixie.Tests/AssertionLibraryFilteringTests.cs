using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Conventions;
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

                var convention = SelfTestConvention.Build();

                var conventionRunner = new ConventionRunner();
                conventionRunner.Run(convention, listener, typeof(SampleTestClass));

                console
                    .Output.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                    .ShouldEqual(
                        "Test 'Fixie.Tests.AssertionLibraryFilteringTests+SampleTestClass.DivideByZero' failed: System.DivideByZeroException",
                        "Attempted to divide by zero.",
                        "   at Fixie.Tests.AssertionLibraryFilteringTests.SampleTestClass.DivideByZero() in " + PathToThisFile() + ":line #",
                        "",
                        "Test 'Fixie.Tests.AssertionLibraryFilteringTests+SampleTestClass.FailedAssertion' failed: Fixie.Tests.SampleAssertionLibrary.AssertionException",
                        "Expected 1, but was 0.",
                        "   at Fixie.Tests.SampleAssertionLibrary.SampleAssert.AreEqual(Int32 expected, Int32 actual) in " + PathToThisFile() + ":line #",
                        "   at Fixie.Tests.AssertionLibraryFilteringTests.SampleTestClass.FailedAssertion() in " + PathToThisFile() + ":line #",
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

                var conventionRunner = new ConventionRunner();
                conventionRunner.Run(convention, listener, typeof(SampleTestClass));

                console
                    .Output.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                    .ShouldEqual(
                        "Test 'Fixie.Tests.AssertionLibraryFilteringTests+SampleTestClass.DivideByZero' failed: System.DivideByZeroException",
                        "Attempted to divide by zero.",
                        "   at Fixie.Tests.AssertionLibraryFilteringTests.SampleTestClass.DivideByZero() in " + PathToThisFile() + ":line #",
                        "",
                        "Test 'Fixie.Tests.AssertionLibraryFilteringTests+SampleTestClass.FailedAssertion' failed: ",
                        "Expected 1, but was 0.",
                        "   at Fixie.Tests.AssertionLibraryFilteringTests.SampleTestClass.FailedAssertion() in " + PathToThisFile() + ":line #",
                        "",
                        "");
            }
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