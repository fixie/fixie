namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Runner.Contracts;

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

            foreach (var assemblyPath in sources)
            {
                try
                {
                    if (AssemblyDirectoryContainsFixie(assemblyPath))
                    {
                        log.Info("Processing " + assemblyPath);

                        Run(assemblyPath, log, frameworkHandle);
                    }
                    else
                    {
                        log.Info("Skipping " + assemblyPath + " because it is not a test assembly.");
                    }
                }
                catch (Exception exception)
                {
                    log.Error(exception);
                }
            }
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

                try
                {
                    if (AssemblyDirectoryContainsFixie(assemblyPath))
                    {
                        log.Info("Processing " + assemblyPath);

                        Run(assemblyPath, log, frameworkHandle, assemblyGroup.Select(x => x.FullyQualifiedName).ToArray());
                    }
                    else
                    {
                        log.Info("Skipping " + assemblyPath + " because it is not a test assembly.");
                    }
                }
                catch (Exception exception)
                {
                    log.Error(exception);
                }
            }
        }

        public void Cancel() { }

        static void HandlePoorVisualStudioImplementationDetails(IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            RemotingUtility.CleanUpRegisteredChannels();

            if (runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;
        }

        static bool AssemblyDirectoryContainsFixie(string assemblyPath)
        {
            return File.Exists(Path.Combine(Path.GetDirectoryName(assemblyPath), "Fixie.dll"));
        }

        static void Run(string assemblyPath, IMessageLogger log, IFrameworkHandle executionSink)
        {
            Run(assemblyPath, log, executionSink, new string[] { });
        }

        static void Run(string assemblyPath, IMessageLogger log, IFrameworkHandle executionSink, string[] testsToRun)
        {
            var runnerChannel = new RunnerChannel();
            var port = runnerChannel.HandleMessagesOnBackgroundThread(MessageHandler(assemblyPath, log, executionSink, testsToRun), log);

            new RunnerProcess(log, assemblyPath, "--designtime", "--port", $"{port}", "--wait-command")
                .Run();

            log.Info("Waiting for background thread to exit.");
            runnerChannel.WaitForBackgroundThread();
        }

        static Action<Message, Action<Message>> MessageHandler(string assemblyPath, IMessageLogger log, IFrameworkHandle executionSink, string[] testsToRun)
        {
            return (message, send) =>
            {
                if (message.MessageType == "TestRunner.WaitingCommand")
                {
                    send(new Message
                    {
                        MessageType = "TestRunner.Execute",
                        Payload = JToken.FromObject(new RunTestsMessage
                        {
                            Tests = new List<string>(testsToRun)
                        })
                    });
                }
                else if (message.MessageType == "TestExecution.TestStarted")
                {
                    executionSink.RecordStart(message.Payload.ToObject<Test>().ToVisualStudioType(assemblyPath));
                }
                else if (message.MessageType == "TestExecution.TestResult")
                {
                    executionSink.RecordResult(message.Payload.ToObject<Runner.Contracts.TestResult>().ToVisualStudioType(assemblyPath));
                }
                else if (message.MessageType == "TestRunner.TestCompleted")
                {
                    log.Info("Test execution completed.");
                }
                else
                {
                    log.Info("Unexpected message:");
                    log.Info(JsonConvert.SerializeObject(message));
                }
            };
        }
    }
}