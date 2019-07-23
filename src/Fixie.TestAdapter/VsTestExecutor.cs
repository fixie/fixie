namespace Fixie.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.IO.Pipes;
    using System.Linq;
    using Internal.Listeners;
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
        /// Called by the IDE, when running all tests.
        /// Called by TFS Build, when running all tests.
        /// Called by TFS Build, with a filter within the run context, when running selected tests.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="runContext"></param>
        /// <param name="frameworkHandle"></param>
        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            try
            {
                IMessageLogger log = frameworkHandle;

                log.Version();

                HandlePoorVsTestImplementationDetails(runContext, frameworkHandle);

                var runAllTests = new PipeMessage.ExecuteTests
                {
                    Filter = new PipeMessage.Test[] { }
                };

                foreach (var assemblyPath in sources)
                    RunTests(log, frameworkHandle, assemblyPath, pipe => pipe.Send(runAllTests));
            }
            catch (Exception exception)
            {
                throw new RunnerException(exception);
            }
        }

        /// <summary>
        /// Called by the IDE, when running selected tests.
        /// Never called from TFS Build.
        /// </summary>
        /// <param name="tests"></param>
        /// <param name="runContext"></param>
        /// <param name="frameworkHandle"></param>
        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            try
            {
                IMessageLogger log = frameworkHandle;

                log.Version();

                HandlePoorVsTestImplementationDetails(runContext, frameworkHandle);

                var assemblyGroups = tests.GroupBy(tc => tc.Source);

                foreach (var assemblyGroup in assemblyGroups)
                {
                    var assemblyPath = assemblyGroup.Key;

                    RunTests(log, frameworkHandle, assemblyPath, pipe =>
                    {
                        pipe.Send(new PipeMessage.ExecuteTests
                        {
                            Filter = assemblyGroup.Select(x =>
                            {
                                var test = new Test(x.FullyQualifiedName);

                                return new PipeMessage.Test
                                {
                                    Class = test.Class,
                                    Method = test.Method,
                                    Name = test.Name
                                };
                            }).ToArray()
                        });
                    });
                }
            }
            catch (Exception exception)
            {
                throw new RunnerException(exception);
            }
        }

        public void Cancel() { }

        static void RunTests(IMessageLogger log, IFrameworkHandle frameworkHandle, string assemblyPath, Action<NamedPipeServerStream> sendCommand)
        {
            if (!IsTestAssembly(assemblyPath))
            {
                log.Info("Skipping " + assemblyPath + " because it is not a test assembly.");
                return;
            }

            log.Info("Processing " + assemblyPath);

            var pipeName = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable("FIXIE_NAMED_PIPE", pipeName);

            using (var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message))
            using (var process = Start(assemblyPath, frameworkHandle))
            {
                pipe.WaitForConnection();

                sendCommand(pipe);

                var recorder = new ExecutionRecorder(frameworkHandle, assemblyPath);

                PipeMessage.CaseStarted lastCaseStarted = null;

                while (true)
                {
                    var messageType = pipe.ReceiveMessage();

                    if (messageType == typeof(PipeMessage.CaseStarted).FullName)
                    {
                        var message = pipe.Receive<PipeMessage.CaseStarted>();
                        lastCaseStarted = message;
                        recorder.Record(message);
                    }
                    else if (messageType == typeof(PipeMessage.CaseSkipped).FullName)
                    {
                        var testResult = pipe.Receive<PipeMessage.CaseSkipped>();
                        recorder.Record(testResult);
                    }
                    else if (messageType == typeof(PipeMessage.CasePassed).FullName)
                    {
                        var testResult = pipe.Receive<PipeMessage.CasePassed>();
                        recorder.Record(testResult);
                    }
                    else if (messageType == typeof(PipeMessage.CaseFailed).FullName)
                    {
                        var testResult = pipe.Receive<PipeMessage.CaseFailed>();
                        recorder.Record(testResult);
                    }
                    else if (messageType == typeof(PipeMessage.Exception).FullName)
                    {
                        var exception = pipe.Receive<PipeMessage.Exception>();
                        throw new RunnerException(exception);
                    }
                    else if (messageType == typeof(PipeMessage.Completed).FullName)
                    {
                        var completed = pipe.Receive<PipeMessage.Completed>();
                        break;
                    }
                    else if (!string.IsNullOrEmpty(messageType))
                    {
                        var body = pipe.ReceiveMessage();
                        log.Error($"The test runner received an unexpected message of type {messageType}: {body}");
                    }
                    else
                    {
                        var exception = new TestProcessExitException(process.TryGetExitCode());

                        if (lastCaseStarted != null)
                        {
                            recorder.Record(new PipeMessage.CaseFailed
                            {
                                Test = lastCaseStarted.Test,
                                Name = lastCaseStarted.Name,
                                Exception = new PipeMessage.Exception(exception)
                            });
                        }

                        throw exception;
                    }
                }
            }
        }

        static void HandlePoorVsTestImplementationDetails(IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;
        }
    }
}