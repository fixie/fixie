using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using static Fixie.TestAdapter.TestAssembly;

namespace Fixie.TestAdapter;

[ExtensionUri(Id)]
public class VsTestExecutor : ITestExecutor
{
    public const string Id = "executor://fixie.testadapter/";
    public static readonly Uri Uri = new(Id);

    /// <summary>
    /// This method was originally intended to be called by VsTest when running
    /// all tests. However, VsTest no longer appears to call this method, instead
    /// favoring its overload with all individual tests specified as if they were
    /// all selected by the user. The stated reason is for performance, but in
    /// fact requires far larger messages to be passed between processes and far
    /// more cross-referencing of test names within each specific test framework
    /// at execution time. This overload is maintained optimistically and for
    /// protection in the event that VsTest changes back to the more efficient
    /// approach.
    /// </summary>
    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(frameworkHandle);

        try
        {
            IMessageLogger log = frameworkHandle;

            log.Version();

            HandlePoorVsTestImplementationDetails(runContext, frameworkHandle);

            foreach (var assemblyPath in sources)
                RunTests(log, frameworkHandle, assemblyPath);
        }
        catch (Exception exception)
        {
            throw new RunnerException(exception);
        }
    }

    /// <summary>
    /// This method was originally intended to be called by VsTest only when running
    /// a selected subset of previously discovered tests. However, VsTest appears to
    /// call this method even in the event the user is running *all* tests, with all
    /// individual tests specified in the input as if all were individually selected
    /// by the user. The stated reason is for performance, but in fact requires far
    /// larger messages to be passed between processes and far more cross-referencing
    /// of test names within each specific test framework at execution time. Still,
    /// this overload is functionally correct even when all tests are passed to it.
    /// </summary>
    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(frameworkHandle);

        try
        {
            IMessageLogger log = frameworkHandle;

            log.Version();

            HandlePoorVsTestImplementationDetails(runContext, frameworkHandle);

            var assemblyGroups = tests.GroupBy(tc => tc.Source);

            foreach (var assemblyGroup in assemblyGroups)
            {
                var assemblyPath = assemblyGroup.Key;

                RunTests(log, frameworkHandle, assemblyPath);
            }
        }
        catch (Exception exception)
        {
            throw new RunnerException(exception);
        }
    }

    public void Cancel() { }

    static void RunTests(IMessageLogger log, IFrameworkHandle frameworkHandle, string assemblyPath)
    {
        if (!IsTestAssembly(assemblyPath))
        {
            log.Info("Skipping " + assemblyPath + " because it is not a test assembly.");
            return;
        }

        log.Info("Processing " + assemblyPath);
    }

    static void HandlePoorVsTestImplementationDetails(IRunContext? runContext, IFrameworkHandle frameworkHandle)
    {
        if (runContext?.KeepAlive == true)
            frameworkHandle.EnableShutdownAfterTestRun = true;
    }
}