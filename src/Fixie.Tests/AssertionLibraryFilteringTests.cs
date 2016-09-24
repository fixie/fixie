﻿namespace Fixie.Tests
{
    using System;
    using System.Runtime.CompilerServices;
    using Fixie.Execution;
    using Fixie.Execution.Listeners;
    using static Utility;

    public class AssertionLibraryFilteringTests
    {
        readonly string testClass = FullName<SampleTestClass>();

        public void ShouldNotAffectOutputByDefault()
        {
            var listener = new ConsoleListener();
            var convention = SelfTestConvention.Build();

            using (var console = new RedirectedConsole())
            {
                Run<SampleTestClass>(listener, convention);

                console
                    .Output
                    .CleanStackTraceLineNumbers()
                    .CleanDuration()
                    .Lines()
                    .ShouldEqual(
                        "------ Testing Assembly Fixie.Tests.dll ------",
                        "",
                        "Test '" + testClass + ".DivideByZero' failed: System.DivideByZeroException",
                        "Attempted to divide by zero.",
                        At<SampleTestClass>("DivideByZero()"),
                        "",
                        "Test '" + testClass + ".FailedAssertion' failed: Fixie.Tests.SampleAssertionLibrary.AssertionException",
                        "Expected 1, but was 0.",
                        "   at Fixie.Tests.SampleAssertionLibrary.SampleAssert.AreEqual(Int32 expected, Int32 actual) in " + PathToThisFile() + ":line #",
                        At<SampleTestClass>("FailedAssertion()"),
                        "",
                        "0 passed, 2 failed, took 1.23 seconds (" + Framework.Version + ").");
            }
        }

        public void ShouldFilterAssertionLibraryImplementationDetailsWhenLibraryTypesAreSpecified()
        {
            var listener = new ConsoleListener();
            var convention = SelfTestConvention.Build();

            convention
                .HideExceptionDetails
                .For<SampleAssertionLibrary.AssertionException>()
                .For(typeof(SampleAssertionLibrary.SampleAssert));

            using (var console = new RedirectedConsole())
            {
                Run<SampleTestClass>(listener, convention);

                console
                    .Output
                    .CleanStackTraceLineNumbers()
                    .CleanDuration()
                    .Lines()
                    .ShouldEqual(
                        "------ Testing Assembly Fixie.Tests.dll ------",
                        "",
                        "Test '" + testClass + ".DivideByZero' failed: System.DivideByZeroException",
                        "Attempted to divide by zero.",
                        At<SampleTestClass>("DivideByZero()"),
                        "",
                        "Test '" + testClass + ".FailedAssertion' failed:",
                        "Expected 1, but was 0.",
                        At<SampleTestClass>("FailedAssertion()"),
                        "",
                        "0 passed, 2 failed, took 1.23 seconds (" + Framework.Version + ").");
            }
        }

        class SampleTestClass
        {
            public void DivideByZero()
            {
                int x = 0;
                x = x / x;
            }

            [MethodImplAttribute(MethodImplOptions.NoInlining)]
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
                    throw new AssertionException($"Expected {expected}, but was {actual}.");
            }
        }

        public class AssertionException : Exception
        {
            public AssertionException(string message)
                : base(message) { }
        }
    }
}