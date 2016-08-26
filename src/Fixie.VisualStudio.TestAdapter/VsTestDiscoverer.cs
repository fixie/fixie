namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Runner.Contracts;

    [DefaultExecutorUri(VsTestExecutor.Id)]
    [FileExtension(".exe")]
    [FileExtension(".dll")]
    public class VsTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger log, ITestCaseDiscoverySink discoverySink)
        {
            log.Version();

            RemotingUtility.CleanUpRegisteredChannels();

            foreach (var assemblyPath in sources)
            {
                try
                {
                    if (AssemblyDirectoryContainsFixie(assemblyPath))
                    {
                        log.Info("Processing " + assemblyPath);

                        DiscoverTests(assemblyPath, log, discoverySink);
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

        static bool AssemblyDirectoryContainsFixie(string assemblyPath)
        {
            return File.Exists(Path.Combine(Path.GetDirectoryName(assemblyPath), "Fixie.dll"));
        }

        static void DiscoverTests(string assemblyPath, IMessageLogger log, ITestCaseDiscoverySink discoverySink)
        {
            using (var messages = new MessageQueue())
            using (var testRunnerChannel = RunnerChannel.CreateAndListen(messages))
            {
                testRunnerChannel.EnqueueMessagesOnBackgroundThread();

                var port = testRunnerChannel.Port;

                new RunnerProcess(log, assemblyPath, "--list", "--designtime", "--port", $"{port}")
                    .Start();

                Message message;
                while (messages.TryTake(out message))
                {
                    if (message.MessageType == "TestDiscovery.TestFound")
                    {
                        discoverySink.SendTestCase(message.Payload.ToObject<Test>().ToVisualStudioType(assemblyPath));
                    }
                    else if (message.MessageType == "TestRunner.TestCompleted")
                    {
                        log.Info("Test discovery completed.");
                        break;
                    }
                    else
                    {
                        log.Info("Unexpected message type: " + message.MessageType);
                    }
                }
            }
        }
    }
}
