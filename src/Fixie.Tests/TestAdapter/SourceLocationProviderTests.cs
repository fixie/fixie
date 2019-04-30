namespace Fixie.Tests.TestAdapter
{
    using Fixie.TestAdapter;
    using Assertions;
    using static Utility;

    public class SourceLocationProviderTests
    {
        // Line numbers vary slightly between Debug and Release modes.
        //
        //      Debug: opening curly brace.
        //      Release: first non-comment code line or closing curly brace if the method is empty.

        static readonly string TestAssemblyPath = typeof(SourceLocationSamples).Assembly.Location;

        public void ShouldSafelyFailForUnknownMethods()
        {
            AssertNoLineNumber("NonExistentClass", "NonExistentMethod");
            AssertNoLineNumber(FullName<SourceLocationSamples>(), "NonExistentMethod");
        }

        public void ShouldDetectLineNumbersOfEmptyMethods()
        {
            AssertLineNumber(FullName<SourceLocationSamples>(), "Empty_OneLine", 8);
            AssertLineNumber(FullName<SourceLocationSamples>(), "Empty_TwoLines", 11);
            AssertLineNumber(FullName<SourceLocationSamples>(), "Empty_ThreeLines", 15);
        }

        public void ShouldDetectLineNumbersOfSynchronousMethods()
        {
            AssertLineNumber(FullName<SourceLocationSamples>(), "Simple", 20);
            AssertLineNumber(FullName<SourceLocationSamples>(), "Generic", 26);
        }

        public void ShouldDetectLineNumbersOfAsyncMethods()
        {
            AssertLineNumber(FullName<SourceLocationSamples>(), "AsyncMethod_Void", 31);
            AssertLineNumber(FullName<SourceLocationSamples>(), "AsyncMethod_Task", 38);
            AssertLineNumber(FullName<SourceLocationSamples>(), "AsyncMethod_TaskOfT", 45);
        }

        public void ShouldDetectLineNumbersOfMethodsWithinNestedClasses()
        {
            AssertLineNumber(FullName<SourceLocationSamples.NestedClass>(), "NestedMethod", 54);
        }

        public void ShouldSafelyFailForUnknownLineNumbers()
        {
            AssertNoLineNumber(FullName<SourceLocationSamples>(), "Hidden");
        }

        public void ShouldDetectLineNumberOfFirstOccurrenceOfOverloadedMethods()
        {
            // Visual Studio's test running infrastructure is incapable of distinguishing
            // overloads, even if we were to report accurate line numbers for each individually.
            // The compromise that all major .NET test frameworks have to make is to report
            // the line number of *one* of the overload occurrences.

            AssertLineNumber(FullName<SourceLocationSamples>(), "Overloaded", 66);
        }

        public void ShouldSafelyFailForInheritedMethodsBecauseTheRequestIsAmbiguous()
        {
            AssertLineNumber(FullName<SourceLocationSamples.BaseClass>(), "Inherited", 72);
            AssertNoLineNumber(FullName<SourceLocationSamples.ChildClass>(), "Inherited");
        }

        static void AssertNoLineNumber(string className, string methodName)
        {
            var sourceLocationProvider = new SourceLocationProvider(TestAssemblyPath);

            var success = sourceLocationProvider.TryGetSourceLocation(className, methodName, out var location);

            success.ShouldBe(false);
            location.ShouldBe(null);
        }

        static void AssertLineNumber(string className, string methodName, int expectedLine)
        {
            var sourceLocationProvider = new SourceLocationProvider(TestAssemblyPath);

            var success = sourceLocationProvider.TryGetSourceLocation(className, methodName, out var location);

            success.ShouldBe(true);
            location.CodeFilePath.EndsWith("SourceLocationSamples.cs").ShouldBe(true);

            location.LineNumber.ShouldBe(expectedLine);
        }
    }
}