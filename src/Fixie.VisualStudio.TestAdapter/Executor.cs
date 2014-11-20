using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.VisualStudio.TestAdapter
{
    [ExtensionUri(Id)]
    public class Executor : ITestExecutor
    {
        public const string Id = "executor://Fixie.VisualStudio";
        public static Uri Uri = new Uri(Id);

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            HandlePoorVisualStudioImplementationDetails(runContext, frameworkHandle);

            IMessageLogger log = frameworkHandle;

            foreach (var source in sources)
            {
                log.Info("Processing " + source);

                try
                {
                    var assemblyFullPath = Path.GetFullPath(source);

                    using (var environment = new ExecutionEnvironment(assemblyFullPath))
                    {
                        var runner = environment.Create<ExecutionProxy>();
                        runner.RunAssembly(assemblyFullPath, new string[] { }, new VisualStudioListener(frameworkHandle, source));
                    }
                }
                catch (Exception exception)
                {
                    log.Error(exception);
                }
            }
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            HandlePoorVisualStudioImplementationDetails(runContext, frameworkHandle);

            IMessageLogger log = frameworkHandle;

            var assemblyGroups = tests.GroupBy(tc => tc.Source);

            foreach (var assemblyGroup in assemblyGroups)
            {
                foreach (var test in assemblyGroup)
                {
                    var message = new StringBuilder()
                        .AppendLine("Source: " + test.Source)
                        .AppendLine("Id: " + test.Id)
                        .AppendLine("DisplayName: " + test.DisplayName)
                        .AppendLine("CodeFilePath: " + test.CodeFilePath)
                        .AppendLine("LineNumber: " + test.LineNumber)
                        .AppendLine("FullyQualifiedName: " + test.FullyQualifiedName)
                        .AppendLine("ExecutorUri: " + test.ExecutorUri.OriginalString)
                        .AppendLine()
                        .ToString();

                    log.Info(message);
                }

                var source = assemblyGroup.Key;

                log.Info("Processing " + source);

                try
                {
                    var assemblyFullPath = Path.GetFullPath(source);

                    using (var environment = new ExecutionEnvironment(assemblyFullPath))
                    {
                        var runner = environment.Create<ExecutionProxy>();
                        runner.RunAssembly(assemblyFullPath, new string[] { }, new VisualStudioListener(frameworkHandle, source));
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
    }
}