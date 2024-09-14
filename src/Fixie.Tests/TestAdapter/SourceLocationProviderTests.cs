using Fixie.TestAdapter;
using static Fixie.Tests.Utility;
using Fixie.Tests.Assertions;
namespace Fixie.Tests.TestAdapter;

public class SourceLocationProviderTests
{
    // Ensure that during the assembly-wide scan of types, methods with no possible
    // body definition are present. This way, we can be sure that method source location
    // scanning safely bypasses these irrelevant methods.
    interface SampleInterface { void MethodWithNoBody(); }
    abstract class SampleAbstractClass { public abstract void MethodWithNoBody(); }

    // Line numbers vary slightly between Debug and Release modes.
    //
    //      Debug: opening curly brace.
    //      Release: first non-comment code line or closing curly brace if the method is empty.

    static readonly string TestAssemblyPath = typeof(SourceLocationSamples).Assembly.Location;

    public void ShouldSafelyFailForUnknownMethods()
    {
        AssertNoLineNumber("NonExistentClass.NonExistentMethod");
        AssertNoLineNumber(FullName<SourceLocationSamples>() + ".NonExistentMethod");
    }

    public void ShouldFailForUnparsableTestNames()
    {
        AssertNoLineNumber("TestNameUnparsableAsFullyQualifiedMethodName");
        AssertNoLineNumber("TestNameUnparsableAsFullyQualifiedMethodNameEndingWithDot.");
        AssertNoLineNumber(".TestNameUnparsableAsFullyQualifiedMethodNameStartingWithDot");
    }

    public void ShouldDetectLineNumbersOfEmptyMethods()
    {
        AssertLineNumber(FullName<SourceLocationSamples>() + ".Empty_OneLine", 8, 8);
        AssertLineNumber(FullName<SourceLocationSamples>() + ".Empty_TwoLines", 11, 12);
        AssertLineNumber(FullName<SourceLocationSamples>() + ".Empty_ThreeLines", 15, 17);
    }

    public void ShouldDetectLineNumbersOfSynchronousMethods()
    {
        AssertLineNumber(FullName<SourceLocationSamples>() + ".Simple", 20, 21);
        AssertLineNumber(FullName<SourceLocationSamples>() + ".Generic", 26, 27);
    }

    public void ShouldDetectLineNumbersOfAsyncMethods()
    {
        AssertLineNumber(FullName<SourceLocationSamples>() + ".AsyncMethod_Void", 31, 32);
        AssertLineNumber(FullName<SourceLocationSamples>() + ".AsyncMethod_Task", 38, 39);
        AssertLineNumber(FullName<SourceLocationSamples>() + ".AsyncMethod_TaskOfT", 45, 46);
    }

    public void ShouldDetectLineNumbersOfMethodsWithinNestedClasses()
    {
        AssertLineNumber(FullName<SourceLocationSamples.NestedClass>() + ".NestedMethod", 54, 55);
    }

    public void ShouldSafelyFailForUnknownLineNumbers()
    {
        AssertNoLineNumber(FullName<SourceLocationSamples>() + ".Hidden");
    }

    public void ShouldDetectLineNumberOfFirstOccurrenceOfOverloadedMethods()
    {
        // VsTest's test running infrastructure is incapable of distinguishing
        // overloads, even if we were to report accurate line numbers for each individually.
        // The compromise that all major .NET test frameworks have to make is to report
        // the line number of *one* of the overload occurrences.

        AssertLineNumber(FullName<SourceLocationSamples>() + ".Overloaded", 66, 66);
    }

    public void ShouldSafelyFailForInheritedMethodsBecauseTheRequestIsAmbiguous()
    {
        AssertLineNumber(FullName<SourceLocationSamples.BaseClass>() + ".Inherited", 72, 72);
        AssertNoLineNumber(FullName<SourceLocationSamples.ChildClass>() + ".Inherited");
    }

    static void AssertNoLineNumber(string test)
    {
        var sourceLocationProvider = new SourceLocationProvider(TestAssemblyPath);

        var success = sourceLocationProvider.TryGetSourceLocation(test, out var location);

        success.ShouldBe(false);
        location.ShouldBe(null);
    }

    static void AssertLineNumber(string test, int debugLine, int releaseLine)
    {
        var sourceLocationProvider = new SourceLocationProvider(TestAssemblyPath);

        if (!sourceLocationProvider.TryGetSourceLocation(test, out var location))
            throw new Exception($"Expected to find a SourceLocation for method {test}.");

        location.CodeFilePath.EndsWith("SourceLocationSamples.cs").ShouldBe(true);
            
#if DEBUG
        location.LineNumber.ShouldBe(debugLine);
#else
        location.LineNumber.ShouldBe(releaseLine);
#endif
    }
}