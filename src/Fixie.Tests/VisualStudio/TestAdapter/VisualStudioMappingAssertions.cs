namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System;
    using Assertions;
    using Fixie.VisualStudio.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;

    public static class VisualStudioMappingAssertions
    {
        public static void ShouldBeDiscoveryTimeTestCase(this TestCase testCase, string expectedAssemblyPath, string expectedFullyQualifiedName)
        {
            ShouldHaveIdentity(testCase, expectedAssemblyPath, expectedFullyQualifiedName);

            ShouldUseDefaultsForUnmappedProperties(testCase);

            ShouldHaveSourceLocation(testCase);
        }

        public static void ShouldBeDiscoveryTimeTestCaseMissingSourceLocation(this TestCase testCase, string expectedAssemblyPath, string expectedFullyQualifiedName)
        {
            ShouldHaveIdentity(testCase, expectedAssemblyPath, expectedFullyQualifiedName);

            ShouldUseDefaultsForUnmappedProperties(testCase);

            ShouldNotHaveSourceLocation(testCase);
        }

        public static void ShouldBeExecutionTimeTestCase(this TestCase testCase, string expectedAssemblyPath, string expectedFullyQualifiedName)
        {
            ShouldHaveIdentity(testCase, expectedAssemblyPath, expectedFullyQualifiedName);

            ShouldUseDefaultsForUnmappedProperties(testCase);

            //Source locations are a discovery-time concern.
            ShouldNotHaveSourceLocation(testCase);
        }

        static void ShouldHaveIdentity(TestCase testCase, string expectedAssemblyPath, string expectedFullyQualifiedName)
        {
            testCase.Source.ShouldEqual(expectedAssemblyPath);
            testCase.FullyQualifiedName.ShouldEqual(expectedFullyQualifiedName);
            testCase.DisplayName.ShouldEqual(testCase.FullyQualifiedName);
        }

        static void ShouldUseDefaultsForUnmappedProperties(TestCase testCase)
        {
            testCase.LocalExtensionData.ShouldBeNull();
            testCase.Id.ShouldNotEqual(Guid.Empty);
            testCase.ExecutorUri.ShouldEqual(VsTestExecutor.Uri);
            testCase.Traits.ShouldBeEmpty();
        }

        static void ShouldHaveSourceLocation(TestCase testCase)
        {
            testCase.CodeFilePath.EndsWith("MessagingTests.cs").ShouldBeTrue();
            testCase.LineNumber.ShouldBeGreaterThan(0);
        }

        static void ShouldNotHaveSourceLocation(TestCase testCase)
        {
            testCase.CodeFilePath.ShouldBeNull();
            testCase.LineNumber.ShouldEqual(-1);
        }
    }
}