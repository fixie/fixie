namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System;
    using Assertions;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;

    public static class TestCaseMappingAssertions
    {
        public static void ShouldBeDiscoveryTimeTest(this TestCase test, string expectedFullyQualifiedName, string expectedSource)
        {
            ShouldHaveIdentity(test, expectedFullyQualifiedName, expectedSource);

            ShouldUseDefaultsForUnmappedProperties(test);

            ShouldHaveSourceLocation(test);
        }

        public static void ShouldBeDiscoveryTimeTestMissingSourceLocation(this TestCase test, string expectedFullyQualifiedName, string expectedSource)
        {
            ShouldHaveIdentity(test, expectedFullyQualifiedName, expectedSource);

            ShouldUseDefaultsForUnmappedProperties(test);

            ShouldNotHaveSourceLocation(test);
        }

        public static void ShouldBeExecutionTimeTest(this TestCase test, string expectedFullyQualifiedName, string expectedSource)
        {
            ShouldHaveIdentity(test, expectedFullyQualifiedName, expectedSource);

            ShouldUseDefaultsForUnmappedProperties(test);

            //Source locations are a discovery-time concern.
            ShouldNotHaveSourceLocation(test);
        }

        static void ShouldHaveIdentity(TestCase test, string expectedFullyQualifiedName, string expectedSource)
        {
            test.FullyQualifiedName.ShouldEqual(expectedFullyQualifiedName);
            test.DisplayName.ShouldEqual(test.FullyQualifiedName);
            test.Source.ShouldEqual(expectedSource);
        }

        static void ShouldUseDefaultsForUnmappedProperties(TestCase test)
        {
            test.LocalExtensionData.ShouldBeNull();
            test.Traits.ShouldBeEmpty();
            test.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
        }

        static void ShouldHaveSourceLocation(TestCase test)
        {
            test.CodeFilePath.EndsWith("MessagingTests.cs").ShouldBeTrue();
            test.LineNumber.ShouldBeGreaterThan(0);
        }

        static void ShouldNotHaveSourceLocation(TestCase test)
        {
            test.CodeFilePath.ShouldBeNull();
            test.LineNumber.ShouldEqual(-1);
        }
    }
}