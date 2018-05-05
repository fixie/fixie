namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.IO.Pipes;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Execution.Listeners;
    using static TestAssembly;

    [DefaultExecutorUri(VsTestExecutor.Id)]
    [FileExtension(".exe")]
    [FileExtension(".dll")]
    public class VsTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger log, ITestCaseDiscoverySink discoverySink)
        {
            log.Version();

            foreach (var assemblyPath in sources)
                DiscoverTests(log, discoverySink, assemblyPath);
        }

        static void DiscoverTests(IMessageLogger log, ITestCaseDiscoverySink discoverySink, string assemblyPath)
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
                Start(assemblyPath);

                pipe.WaitForConnection();

                pipe.Send<PipeMessage.DiscoverTests>();

                var recorder = new DiscoveryRecorder(log, discoverySink, assemblyPath);

                while (true)
                {
                    var messageType = pipe.ReceiveMessage();

                    if (messageType == typeof(PipeMessage.TestDiscovered).FullName)
                    {
                        var testDiscovered = pipe.Receive<PipeMessage.TestDiscovered>();
                        recorder.Record(testDiscovered);
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
    }
}
