using System.Diagnostics;
using System.IO.Pipes;
using Fixie.Internal;
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
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(frameworkHandle);

        IMessageLogger log = frameworkHandle;
        
        try
        {
            log.Version();

            HandlePoorVsTestImplementationDetails(runContext, frameworkHandle);

            var executeTests = new PipeMessage.ExecuteTests
            {
                Filter = []
            };

            foreach (var assemblyPath in sources)
                RunTests(log, frameworkHandle, assemblyPath, executeTests);
        }
        catch (Exception exception)
        {
            throw new RunnerException(exception);
        }

        stopwatch.Stop();
        log.Info($"RunTests[All] took {stopwatch.Elapsed}");
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
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(frameworkHandle);

        IMessageLogger log = frameworkHandle;
        
        try
        {
            log.Version();

            HandlePoorVsTestImplementationDetails(runContext, frameworkHandle);

            var assemblyGroups = tests.GroupBy(tc => tc.Source);

            foreach (var assemblyGroup in assemblyGroups)
            {
                var assemblyPath = assemblyGroup.Key;

                var executeTests = new PipeMessage.ExecuteTests
                {
                    Filter = assemblyGroup.Select(x => x.FullyQualifiedName).ToArray()
                };

                RunTests(log, frameworkHandle, assemblyPath, executeTests);
            }
        }
        catch (Exception exception)
        {
            throw new RunnerException(exception);
        }

        stopwatch.Stop();
        log.Info($"RunTests[Selected] took {stopwatch.Elapsed}");
    }

    public void Cancel() { }

    static void RunTests(IMessageLogger log, IFrameworkHandle frameworkHandle, string assemblyPath, PipeMessage.ExecuteTests executeTests)
    {
        if (!IsTestAssembly(assemblyPath))
        {
            log.Info("Skipping " + assemblyPath + " because it is not a test assembly.");
            return;
        }

        log.Info("Processing " + assemblyPath);

        var pipeName = Guid.NewGuid().ToString();
        Environment.SetEnvironmentVariable("FIXIE_NAMED_PIPE", pipeName);

        using (var pipeStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte))
        using (var pipe = new TestAdapterPipe(pipeStream))
        using (var process = StartExecution(assemblyPath, frameworkHandle, out var attachmentFailure))
        {
            pipeStream.WaitForConnection();

            pipe.Send(executeTests);

            var recorder = new VsExecutionRecorder(frameworkHandle, assemblyPath);

            PipeMessage.TestStarted? lastTestStarted = null;

            while (true)
            {
                var messageType = pipe.ReceiveMessageType();

                if (messageType == typeof(PipeMessage.TestStarted).FullName)
                {
                    var message = pipe.Receive<PipeMessage.TestStarted>();
                    lastTestStarted = message;
                    recorder.Record(message);
                }
                else if (messageType == typeof(PipeMessage.TestSkipped).FullName)
                {
                    var testResult = pipe.Receive<PipeMessage.TestSkipped>();
                    recorder.Record(testResult);
                }
                else if (messageType == typeof(PipeMessage.TestPassed).FullName)
                {
                    var testResult = pipe.Receive<PipeMessage.TestPassed>();
                    recorder.Record(testResult);
                }
                else if (messageType == typeof(PipeMessage.TestFailed).FullName)
                {
                    var testResult = pipe.Receive<PipeMessage.TestFailed>();
                    recorder.Record(testResult);
                }
                else if (messageType == typeof(PipeMessage.Exception).FullName)
                {
                    var exception = pipe.Receive<PipeMessage.Exception>();
                    throw new RunnerException(exception);
                }
                else if (messageType == typeof(PipeMessage.EndOfPipe).FullName)
                {
                    var endOfPipe = pipe.Receive<PipeMessage.EndOfPipe>();
                    break;
                }
                else if (!string.IsNullOrEmpty(messageType))
                {
                    var body = pipe.ReceiveMessageBody();
                    log.Error($"The test runner received an unexpected message of type {messageType}: {body}");
                }
                else
                {
                    var exception = new TestProcessExitException(process.TryGetExitCode());

                    if (lastTestStarted != null)
                    {
                        recorder.Record(new PipeMessage.TestFailed
                        {
                            Test = lastTestStarted.Test,
                            TestCase = lastTestStarted.Test,
                            Reason = new PipeMessage.Exception(exception),
                            DurationInMilliseconds = 0,
                            Output = ""
                        });
                    }

                    throw exception;
                }
            }

            if (attachmentFailure != null)
            {
                var exception = attachmentFailure.ThirdPartyTestHostException;

                var reason = new PipeMessage.Exception
                {
                    Type = exception.GetType().FullName!,
                    Message = attachmentFailure.Message,
                    StackTrace = exception.StackTrace!
                };

                foreach (var selectedTest in executeTests.Filter)
                {
                    recorder.Record(new PipeMessage.TestFailed
                    {
                        Test = selectedTest,
                        TestCase = selectedTest,
                        Reason = reason,
                        DurationInMilliseconds = 0,
                        Output = ""
                    });
                }
            }
        }
    }

    static void HandlePoorVsTestImplementationDetails(IRunContext? runContext, IFrameworkHandle frameworkHandle)
    {
        if (runContext?.KeepAlive == true)
            frameworkHandle.EnableShutdownAfterTestRun = true;
    }
}