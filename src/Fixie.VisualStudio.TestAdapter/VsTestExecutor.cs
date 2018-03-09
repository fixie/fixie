namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.IO.Pipes;
    using System.Linq;
    using Execution.Listeners;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using static TestAssembly;

    [ExtensionUri(Id)]
    public class VsTestExecutor : ITestExecutor
    {
        public const string Id = "executor://Fixie.VisualStudio";
        public static Uri Uri = new Uri(Id);

        /// <summary>
        /// Called by Visual Studio, when running all tests.
        /// Called by TFS Build, when running all tests.
        /// Called by TFS Build, with a filter within the run context, when running selected tests.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="runContext"></param>
        /// <param name="frameworkHandle"></param>
        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            IMessageLogger log = frameworkHandle;

            log.Version();

            HandlePoorVisualStudioImplementationDetails(runContext, frameworkHandle);

            var runAllTests = new PipeMessage.ExecuteTests
            {
                Filter = new PipeMessage.Test[] { }
            };

            foreach (var assemblyPath in sources)
                RunTests(log, frameworkHandle, assemblyPath, pipe => pipe.Send(runAllTests));
        }

        /// <summary>
        /// Called by Visual Studio, when running selected tests.
        /// Never called from TFS Build.
        /// </summary>
        /// <param name="tests"></param>
        /// <param name="runContext"></param>
        /// <param name="frameworkHandle"></param>
        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            IMessageLogger log = frameworkHandle;

            log.Version();

            HandlePoorVisualStudioImplementationDetails(runContext, frameworkHandle);

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
                            var testName = new TestName(x.FullyQualifiedName);

                            return new PipeMessage.Test
                            {
                                Class = testName.Class,
                                Method = testName.Method,
                                Name = testName.FullName
                            };
                        }).ToArray()
                    });
                });
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
            {
                Start(assemblyPath, frameworkHandle);

                pipe.WaitForConnection();

                sendCommand(pipe);

                var recorder = new ExecutionRecorder(frameworkHandle, assemblyPath);

                while (true)
                {
                    var messageType = pipe.ReceiveMessage();

                    if (messageType == typeof(PipeMessage.SkipResult).FullName)
                    {
                        var testResult = pipe.Receive<PipeMessage.SkipResult>();
                        recorder.RecordResult(testResult);
                    }
                    else if (messageType == typeof(PipeMessage.PassResult).FullName)
                    {
                        var testResult = pipe.Receive<PipeMessage.PassResult>();
                        recorder.RecordResult(testResult);
                    }
                    else if (messageType == typeof(PipeMessage.FailResult).FullName)
                    {
                        var testResult = pipe.Receive<PipeMessage.FailResult>();
                        recorder.RecordResult(testResult);
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
                    else
                    {
                        var body = pipe.ReceiveMessage();
                        throw new Exception($"Test runner received unexpected message of type {messageType}: {body}");
                    }
                }
            }
        }

        static void HandlePoorVisualStudioImplementationDetails(IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;
        }
    }
}