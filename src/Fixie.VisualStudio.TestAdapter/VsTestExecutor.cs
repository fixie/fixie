namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using Execution;
    using Execution.Listeners;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

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

                        var pipeName = Guid.NewGuid().ToString();
                        Environment.SetEnvironmentVariable("FIXIE_NAMED_PIPE", pipeName);

                        using (var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message))
                        using (TestAssembly.Start(assemblyPath))
                        {
                            pipe.WaitForConnection();

                            pipe.SendMessage("RunAssembly");

                            var recorder = new ExecutionRecorder(frameworkHandle, assemblyPath);

                            while (true)
                            {
                                var message = pipe.ReceiveMessage();

                                if (message == typeof(TestExplorerListener.TestResult).FullName)
                                    recorder.RecordResult(pipe.Receive<TestExplorerListener.TestResult>());

                                if (message == typeof(TestExplorerListener.Completed).FullName)
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

                        var methods = assemblyGroup.Select(x => x.FullyQualifiedName).ToArray();

                        var pipeName = Guid.NewGuid().ToString();
                        Environment.SetEnvironmentVariable("FIXIE_NAMED_PIPE", pipeName);

                        using (var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message))
                        using (TestAssembly.Start(assemblyPath))
                        {
                            pipe.WaitForConnection();

                            pipe.SendMessage("RunMethods");
                            pipe.Send(new RunMethods {Methods = methods});

                            var recorder = new ExecutionRecorder(frameworkHandle, assemblyPath);

                            while (true)
                            {
                                var message = pipe.ReceiveMessage();

                                if (message == typeof(TestExplorerListener.TestResult).FullName)
                                    recorder.RecordResult(pipe.Receive<TestExplorerListener.TestResult>());

                                if (message == typeof(TestExplorerListener.Completed).FullName)
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

        public void Cancel() { }

        static void HandlePoorVisualStudioImplementationDetails(IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;
        }

        static bool AssemblyDirectoryContainsFixie(string assemblyPath)
        {
            return File.Exists(Path.Combine(Path.GetDirectoryName(assemblyPath), "Fixie.dll"));
        }
    }
}