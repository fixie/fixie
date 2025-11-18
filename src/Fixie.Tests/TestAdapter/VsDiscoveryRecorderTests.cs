using Fixie.Internal;
using Fixie.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Fixie.Tests.Reports;

namespace Fixie.Tests.TestAdapter;

public class VsDiscoveryRecorderTests : MessagingTests
{
    public void ShouldMapDiscoveredTestsToVsTestDiscoverySink()
    {
        var assemblyPath = typeof(MessagingTests).Assembly.Location;

        var discoverySink = new StubTestCaseDiscoverySink();

        var discoveryRecorder = new VsDiscoveryRecorder(discoverySink, assemblyPath);

        RecordAnticipatedPipeMessages(assemblyPath, discoveryRecorder, sourceLocationsExist: true);

        discoverySink.TestCases.ItemsShouldSatisfy([
            x => x.ShouldBeDiscoveryTimeTest(TestClass + ".Fail", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTest(TestClass + ".FailByAssertion", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTest(TestClass + ".Skip", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath)
        ]);
    }

    public void ShouldDefaultSourceLocationPropertiesWhenSourceInspectionThrows()
    {
        const string invalidAssemblyPath = "assembly.path.dll";

        var discoverySink = new StubTestCaseDiscoverySink();

        var discoveryRecorder = new VsDiscoveryRecorder(discoverySink, invalidAssemblyPath);

        RecordAnticipatedPipeMessages(invalidAssemblyPath, discoveryRecorder, sourceLocationsExist: false);

        discoverySink.TestCases.ItemsShouldSatisfy([
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Fail", invalidAssemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".FailByAssertion", invalidAssemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", invalidAssemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Skip", invalidAssemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(GenericTestClass + ".ShouldBeString", invalidAssemblyPath)
        ]);
    }

    void RecordAnticipatedPipeMessages(string assemblyPath, VsDiscoveryRecorder vsDiscoveryRecorder, bool sourceLocationsExist)
    {
        SourceLocationProvider sourceLocationProvider = new(assemblyPath);
        SourceLocation? sourceLocation = null;

        var test = TestClass + ".Fail";
        if (sourceLocationsExist)
            sourceLocationProvider.TryGetSourceLocation(test, out sourceLocation).ShouldBe(true);
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = test,
            SourceLocation = sourceLocation
        });

        test = TestClass + ".FailByAssertion";
        if (sourceLocationsExist)
            sourceLocationProvider.TryGetSourceLocation(test, out sourceLocation).ShouldBe(true);
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = test,
            SourceLocation = sourceLocation
        });

        test = TestClass + ".Pass";
        if (sourceLocationsExist)
            sourceLocationProvider.TryGetSourceLocation(test, out sourceLocation).ShouldBe(false);
        sourceLocation.ShouldBe(null);
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = test,
            SourceLocation = sourceLocation
        });

        test = TestClass + ".Skip";
        if (sourceLocationsExist)
            sourceLocationProvider.TryGetSourceLocation(test, out sourceLocation).ShouldBe(true);
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = test,
            SourceLocation = sourceLocation
        });

        test = GenericTestClass + ".ShouldBeString";
        if (sourceLocationsExist)
            sourceLocationProvider.TryGetSourceLocation(test, out sourceLocation).ShouldBe(true);
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = test,
            SourceLocation = sourceLocation
        });
    }

    class StubTestCaseDiscoverySink : ITestCaseDiscoverySink
    {
        public List<TestCase> TestCases { get; } = [];

        public void SendTestCase(TestCase discoveredTest)
            => TestCases.Add(discoveredTest);
    }
}