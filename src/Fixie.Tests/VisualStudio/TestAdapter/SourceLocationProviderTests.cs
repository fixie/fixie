namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System.IO;
    using Fixie.VisualStudio.TestAdapter;
    using Should;

    public class SourceLocationProviderTests
    {
        // Line numbers vary slightly between Debug and Release modes.
        //
        //      Debug: opening curly brace.
        //      Release: first non-comment code line or closing curly brace if the method is empty.

        private static readonly string TestAssemblyPath = Path.GetFullPath("Fixie.Tests.dll");

        public void ShouldSafelyFailForUnknownMethods()
        {
            AssertNoLineNumber("NonExistentClass", "NonExistentMethod");
            AssertNoLineNumber(typeof(SourceLocationSamples).FullName, "NonExistentMethod");
        }

        public void ShouldDetectLineNumbersOfEmptyMethods()
        {
            AssertLineNumber(typeof(SourceLocationSamples).FullName, "Empty_OneLine", 8, 8);
            AssertLineNumber(typeof(SourceLocationSamples).FullName, "Empty_TwoLines", 11, 12);
            AssertLineNumber(typeof(SourceLocationSamples).FullName, "Empty_ThreeLines", 15, 17);
        }

        public void ShouldDetectLineNumbersOfSynchronousMethods()
        {
            AssertLineNumber(typeof(SourceLocationSamples).FullName, "Simple", 20, 21);
            AssertLineNumber(typeof(SourceLocationSamples).FullName, "Generic", 26, 27);
        }

        public void ShouldDetectLineNumbersOfAsyncMethods()
        {
            AssertLineNumber(typeof(SourceLocationSamples).FullName, "AsyncMethod_Void", 31, 32);
            AssertLineNumber(typeof(SourceLocationSamples).FullName, "AsyncMethod_Task", 38, 39);
            AssertLineNumber(typeof(SourceLocationSamples).FullName, "AsyncMethod_TaskOfT", 45, 46);
        }

        public void ShouldDetectLineNumbersOfMethodsWithinNestedClasses()
        {
            AssertLineNumber(typeof(SourceLocationSamples.NestedClass).FullName, "NestedMethod", 54, 55);
        }

        public void ShouldSafelyFailForUnknownLineNumbers()
        {
            AssertNoLineNumber(typeof(SourceLocationSamples).FullName, "Hidden");
        }

        public void ShouldDetectLineNumberOfFirstOccurrenceOfOverloadedMethods()
        {
            // Visual Studio's test running infrastructure is incapable of distinguishing
            // overloads, even if we were to report accurate line numbers for each individually.
            // The compromise that all major .NET test frameworks have to make is to report
            // the line number of *one* of the overload occurrences.

            AssertLineNumber(typeof(SourceLocationSamples).FullName, "Overloaded", 66, 66);
        }

        public void ShouldSafelyFailForInheritedMethodsBecauseTheRequestIsAmbiguous()
        {
            AssertLineNumber(typeof(SourceLocationSamples.BaseClass).FullName, "Inherited", 72, 72);
            AssertNoLineNumber(typeof(SourceLocationSamples.ChildClass).FullName, "Inherited");
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