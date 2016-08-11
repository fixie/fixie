namespace Fixie.Tests.Runner
{
    using Fixie.Runner;
    using Should;
    using static Utility;

    public class SourceLocationProviderTests
    {
        // Line numbers vary slightly between Debug and Release modes.
        //
        //      Debug: opening curly brace.
        //      Release: first non-comment code line or closing curly brace if the method is empty.

        private static readonly string TestAssemblyPath = typeof(SourceLocationSamples).Assembly.Location;

        public void ShouldSafelyFailForUnknownMethods()
        {
            AssertNoLineNumber("NonExistentClass", "NonExistentMethod");
            AssertNoLineNumber(FullName<SourceLocationSamples>(), "NonExistentMethod");
        }

        public void ShouldDetectLineNumbersOfEmptyMethods()
        {
            AssertLineNumber(FullName<SourceLocationSamples>(), "Empty_OneLine", 8, 8);
            AssertLineNumber(FullName<SourceLocationSamples>(), "Empty_TwoLines", 11, 12);
            AssertLineNumber(FullName<SourceLocationSamples>(), "Empty_ThreeLines", 15, 17);
        }

        public void ShouldDetectLineNumbersOfSynchronousMethods()
        {
            AssertLineNumber(FullName<SourceLocationSamples>(), "Simple", 20, 21);
            AssertLineNumber(FullName<SourceLocationSamples>(), "Generic", 26, 27);
        }

        public void ShouldDetectLineNumbersOfAsyncMethods()
        {
            AssertLineNumber(FullName<SourceLocationSamples>(), "AsyncMethod_Void", 31, 32);
            AssertLineNumber(FullName<SourceLocationSamples>(), "AsyncMethod_Task", 38, 39);
            AssertLineNumber(FullName<SourceLocationSamples>(), "AsyncMethod_TaskOfT", 45, 46);
        }

        public void ShouldDetectLineNumbersOfMethodsWithinNestedClasses()
        {
            AssertLineNumber(FullName<SourceLocationSamples.NestedClass>(), "NestedMethod", 54, 55);
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

            AssertLineNumber(FullName<SourceLocationSamples>(), "Overloaded", 66, 66);
        }

        public void ShouldSafelyFailForInheritedMethodsBecauseTheRequestIsAmbiguous()
        {
            AssertLineNumber(FullName<SourceLocationSamples.BaseClass>(), "Inherited", 72, 72);
            AssertNoLineNumber(FullName<SourceLocationSamples.ChildClass>(), "Inherited");
        }

        static void AssertNoLineNumber(string className, string methodName)
        {
            var sourceLocationProvider = new SourceLocationProvider(TestAssemblyPath);

            SourceLocation location;
            var success = sourceLocationProvider.TryGetSourceLocation(new MethodGroup(className + "." + methodName), out location);

            success.ShouldBeFalse();
            location.ShouldBeNull();
        }

        static void AssertLineNumber(string className, string methodName, int debugLine, int releaseLine)
        {
            var sourceLocationProvider = new SourceLocationProvider(TestAssemblyPath);

            SourceLocation location;
            var success = sourceLocationProvider.TryGetSourceLocation(new MethodGroup(className + "." + methodName), out location);

            success.ShouldBeTrue();
            location.CodeFilePath.EndsWith("SourceLocationSamples.cs").ShouldBeTrue();

#if DEBUG
            location.LineNumber.ShouldEqual(debugLine);
#else
            location.LineNumber.ShouldEqual(releaseLine);
#endif
        }
    }
}