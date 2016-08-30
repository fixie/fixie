namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Newtonsoft.Json;
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
            using (var runnerChannel = new RunnerChannel(log))
            {
                var runnerProcess = new RunnerProcess(log, assemblyPath, "--list", "--designtime", "--port", $"{runnerChannel.Port}");
                runnerProcess.Start();

                runnerChannel.HandleMessages(MessageHandler(assemblyPath, log, discoverySink));

                runnerProcess.WaitForExit();
            }
        }

        static Action<Message, Action<Message>> MessageHandler(string assemblyPath, IMessageLogger log, ITestCaseDiscoverySink discoverySink)
        {
            return (message, send) =>
            {
                if (message.MessageType == "TestDiscovery.TestFound")
                {
                    discoverySink.SendTestCase(message.Payload.ToObject<Test>().ToVisualStudioType(assemblyPath));
                }
                else if (message.MessageType == "TestRunner.TestCompleted")
                {
                    log.Info("Test discovery completed.");
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
