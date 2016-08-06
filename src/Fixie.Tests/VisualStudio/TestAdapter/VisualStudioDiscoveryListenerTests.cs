namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fixie.VisualStudio.TestAdapter;
    using Fixie.VisualStudio.TestAdapter.Wrappers;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Should;

    public class VisualStudioDiscoveryListenerTests : MessagingTests
    {
        public void ShouldReportDiscoveredMethodGroupsToDiscoverySink()
        {
            string assemblyPath = typeof(MessagingTests).Assembly.Location;

            var messageLogger = new StubMessageLogger();
            var testCaseDiscoverySink = new StubTestCaseDiscoverySink();

            Discover(messageLogger, testCaseDiscoverySink, assemblyPath);

            messageLogger.Messages.ShouldBeEmpty();

            var testCases = testCaseDiscoverySink.TestCases.OrderBy(x => x.LineNumber).ToArray();

            foreach (var testCase in testCases)
            {
                testCase.LocalExtensionData.ShouldBeNull();
                testCase.Id.ShouldNotEqual(Guid.Empty);
                testCase.ExecutorUri.ShouldEqual(VsTestExecutor.Uri);
                testCase.Source.ShouldEqual(assemblyPath);
                testCase.DisplayName.ShouldEqual(testCase.FullyQualifiedName);

                testCase.CodeFilePath.EndsWith("MessagingTests.cs").ShouldBeTrue();
                testCase.LineNumber.ShouldBeGreaterThan(0);
            }

            testCases.Select(x => x.FullyQualifiedName)
                .ShouldEqual(
                    TestClass + ".Pass",
                    TestClass + ".Fail",
                    TestClass + ".FailByAssertion",
                    TestClass + ".SkipWithoutReason",
                    TestClass + ".SkipWithReason");
        }

        public void ShouldNotSetSourceLocationPropertiesWhenSourceInspectionThrows()
        {
            const string invalidAssemblyPath = "assembly.path.dll";

            var messageLogger = new StubMessageLogger();
            var testCaseDiscoverySink = new StubTestCaseDiscoverySink();

            Discover(messageLogger, testCaseDiscoverySink, invalidAssemblyPath);

            messageLogger.Messages.Count.ShouldEqual(5);

            var testCases = testCaseDiscoverySink.TestCases.OrderBy(x => x.LineNumber).ToArray();

            foreach (var testCase in testCases)
            {
                testCase.LocalExtensionData.ShouldBeNull();
                testCase.Id.ShouldNotEqual(Guid.Empty);
                testCase.ExecutorUri.ShouldEqual(VsTestExecutor.Uri);
                testCase.Source.ShouldEqual(invalidAssemblyPath);
                testCase.DisplayName.ShouldEqual(testCase.FullyQualifiedName);

                testCase.CodeFilePath.ShouldBeNull();
                testCase.LineNumber.ShouldEqual(-1);
            }

            testCases.Select(x => x.FullyQualifiedName)
                .ShouldEqual(
                    TestClass + ".Pass",
                    TestClass + ".Fail",
                    TestClass + ".FailByAssertion",
                    TestClass + ".SkipWithoutReason",
                    TestClass + ".SkipWithReason");
        }

        void Discover(StubMessageLogger messageLogger, StubTestCaseDiscoverySink testCaseDiscoverySink, string assemblyPath)
        {
            using (var messageLoggerWrapper = new MessageLogger(messageLogger))
            using (var testCaseDiscoverySinkWrapper = new TestCaseDiscoverySink(testCaseDiscoverySink))
            using (var listener = new VisualStudioDiscoveryListener(messageLoggerWrapper, testCaseDiscoverySinkWrapper, assemblyPath))
            {
                Discover(listener);
            }
        }

        class StubMessageLogger : IMessageLogger
        {
            public List<Tuple<TestMessageLevel, string>> Messages { get; }
                = new List<Tuple<TestMessageLevel, string>>();

            public void SendMessage(TestMessageLevel testMessageLevel, string message)
                => Messages.Add(new Tuple<TestMessageLevel, string>(testMessageLevel, message));
        }

        class StubTestCaseDiscoverySink : ITestCaseDiscoverySink
        {
            public List<TestCase> TestCases { get; }
                = new List<TestCase>();

            public void SendTestCase(TestCase discoveredTest)
                => TestCases.Add(discoveredTest);
        }
    }
}