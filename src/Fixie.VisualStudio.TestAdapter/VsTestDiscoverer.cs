namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipes;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Execution.Listeners;

    [DefaultExecutorUri(VsTestExecutor.Id)]
    [FileExtension(".exe")]
    [FileExtension(".dll")]
    public class VsTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger log, ITestCaseDiscoverySink discoverySink)
        {
            log.Version();

            foreach (var assemblyPath in sources)
            {
                try
                {
                    if (AssemblyDirectoryContainsFixie(assemblyPath))
                    {
                        log.Info("Processing " + assemblyPath);

                        var pipeName = Guid.NewGuid().ToString();
                        Environment.SetEnvironmentVariable("FIXIE_NAMED_PIPE", pipeName);

                        using (var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message))
                        using (TestAssembly.Start(assemblyPath))
                        {
                            pipe.WaitForConnection();

                            pipe.SendMessage("DiscoverMethods");

                            var recorder = new DiscoveryRecorder(log, discoverySink, assemblyPath);

                            while (true)
                            {
                                var message = pipe.ReceiveMessage();

                                if (message == typeof(PipeListener.Test).FullName)
                                    recorder.SendTestCase(pipe.Receive<PipeListener.Test>());

                                if (message == typeof(PipeListener.Completed).FullName)
                                    break;
                            }
                        }
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
    }
}
