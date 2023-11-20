namespace Fixie.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Reflection;
    using Internal;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using static TestAssembly;

    [ExtensionUri(Id)]
    public class VsTestExecutor : ITestExecutor
    {
        public const string Id = "executor://fixie.testadapter/";
        public static readonly Uri Uri = new Uri(Id);

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
            try
            {
                if (sources == null)
                    throw new ArgumentNullException(nameof(sources));

                if (frameworkHandle == null)
                    throw new ArgumentNullException(nameof(frameworkHandle));

                IMessageLogger log = frameworkHandle;

                log.Version();

                HandlePoorVsTestImplementationDetails(runContext, frameworkHandle);

                if (false)
                {
                    foreach (var assemblyPath in sources)
                        RunTestsInProcess(log, frameworkHandle, assemblyPath, runner =>
                        {
                            runner.Run().GetAwaiter().GetResult();
                        });
                }
                else
                {
                    var runAllTests = new PipeMessage.ExecuteTests
                    {
                        Filter = new string[] { }
                    };

                    foreach (var assemblyPath in sources)
                        RunTests(log, frameworkHandle, assemblyPath, pipe => pipe.Send(runAllTests));                    
                }
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
            try
            {
                if (tests == null)
                    throw new ArgumentNullException(nameof(tests));

                if (frameworkHandle == null)
                    throw new ArgumentNullException(nameof(frameworkHandle));

                IMessageLogger log = frameworkHandle;

                log.Version();

                HandlePoorVsTestImplementationDetails(runContext, frameworkHandle);

                var assemblyGroups = tests.GroupBy(tc => tc.Source);

                foreach (var assemblyGroup in assemblyGroups)
                {
                    var assemblyPath = assemblyGroup.Key;

                    if (false)
                    {
                        RunTestsInProcess(log, frameworkHandle, assemblyPath, runner =>
                        {
                            var selectedTests =
                                new HashSet<string>(assemblyGroup.Select(x => x.FullyQualifiedName));

                            runner.Run(selectedTests).GetAwaiter().GetResult();
                        });
                    }
                    else
                    {
                        RunTests(log, frameworkHandle, assemblyPath, pipe =>
                        {
                            pipe.Send(new PipeMessage.ExecuteTests
                            {
                                Filter = assemblyGroup.Select(x => x.FullyQualifiedName).ToArray()
                            });
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                throw new RunnerException(exception);
            }
        }

        public void Cancel() { }

        static void RunTests(IMessageLogger log, IFrameworkHandle frameworkHandle, string assemblyPath, Action<TestAdapterPipe> sendCommand)
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
            using (var process = Start(assemblyPath, frameworkHandle))
            {
                pipeStream.WaitForConnection();

                sendCommand(pipe);

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
            }
        }

        static void RunTestsInProcess(IMessageLogger log, IFrameworkHandle frameworkHandle, string assemblyPath, Action<Runner> run)
        {
            if (!IsTestAssembly(assemblyPath))
            {
                log.Info("Skipping " + assemblyPath + " because it is not a test assembly.");
                return;
            }

            log.Info("Processing " + assemblyPath);

            log.Info("In order to run under the debugger, your test assembly is running in-process within the Test Adapter.");
            log.Info("If you experience assembly loading issues at runtime, try instead debugging the test project directly under the debugger, as it is an executable itself.");

            Directory.SetCurrentDirectory(FolderPath(assemblyPath));
            var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath));
            var testAssemblyLoadContext = new TestAssemblyLoadContext(assemblyPath);
            var assembly = testAssemblyLoadContext.LoadFromAssemblyName(assemblyName);
            var report = new InProcessExecutionReport(frameworkHandle, assemblyPath);
            
            var console = Console.Out;
            var rootPath = Directory.GetCurrentDirectory();
            var environment = new TestEnvironment(assembly, console, rootPath);

            using var boundary = new ConsoleRedirectionBoundary();

            var runner = new Runner(environment, report);

            run(runner);
        }

        static void HandlePoorVsTestImplementationDetails(IRunContext? runContext, IFrameworkHandle frameworkHandle)
        {
            if (runContext?.KeepAlive == true)
                frameworkHandle.EnableShutdownAfterTestRun = true;
        }
    }
}