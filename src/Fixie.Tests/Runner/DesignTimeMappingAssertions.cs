namespace Fixie.Tests.Runner
{
    using Assertions;
    using Fixie.Runner.Contracts;

    public static class DesignTimeMappingAssertions
    {
        public static void ShouldBeDiscoveryTimeTest(this Test test, string expectedFullyQualifiedName)
        {
            ShouldHaveIdentity(test, expectedFullyQualifiedName);

            ShouldUseDefaultsForUnmappedProperties(test);

            ShouldHaveSourceLocation(test);
        }

        public static void ShouldBeDiscoveryTimeTestMissingSourceLocation(this Test test, string expectedFullyQualifiedName)
        {
            ShouldHaveIdentity(test, expectedFullyQualifiedName);

            ShouldUseDefaultsForUnmappedProperties(test);

            ShouldNotHaveSourceLocation(test);
        }

        public static void ShouldBeExecutionTimeTest(this Test test, string expectedFullyQualifiedName)
        {
            ShouldHaveIdentity(test, expectedFullyQualifiedName);

            ShouldUseDefaultsForUnmappedProperties(test);

            //Source locations are a discovery-time concern.
            ShouldNotHaveSourceLocation(test);
        }

        static void ShouldHaveIdentity(Test test, string expectedFullyQualifiedName)
        {
            test.FullyQualifiedName.ShouldEqual(expectedFullyQualifiedName);
            test.DisplayName.ShouldEqual(test.FullyQualifiedName);
        }

        static void ShouldUseDefaultsForUnmappedProperties(Test test)
        {
            test.Id.ShouldEqual(null);
            test.Properties.ShouldBeEmpty();
        }

        static void ShouldHaveSourceLocation(Test test)
        {
            test.CodeFilePath.EndsWith("MessagingTests.cs").ShouldBeTrue();
            test.LineNumber.ShouldBeGreaterThan(0);
        }

        static void ShouldNotHaveSourceLocation(Test test)
        {
            test.CodeFilePath.ShouldBeNull();
            test.LineNumber.ShouldBeNull();
        }
    }
}