namespace Fixie.Tests.TestAdapter
{
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
            test.FullyQualifiedName.ShouldBe(expectedFullyQualifiedName);
            test.DisplayName.ShouldBe(test.FullyQualifiedName);
            test.Source.ShouldBe(expectedSource);
        }

        static void ShouldUseDefaultsForUnmappedProperties(TestCase test)
        {
            test.Traits.ShouldBeEmpty();
            test.ExecutorUri.ToString().ShouldBe("executor://fixie.visualstudio/");
        }

        static void ShouldHaveSourceLocation(TestCase test)
        {
            test.CodeFilePath.EndsWith("MessagingTests.cs").ShouldBe(true);
            test.LineNumber.ShouldBeGreaterThan(0);
        }

        static void ShouldNotHaveSourceLocation(TestCase test)
        {
            test.CodeFilePath.ShouldBe(null);
            test.LineNumber.ShouldBe(-1);
        }
    }
}