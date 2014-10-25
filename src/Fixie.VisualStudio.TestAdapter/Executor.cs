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
            frameworkHandle.SendMessage(TestMessageLevel.Informational, "RunTests invoked with string 'sources' list:");

            HandlePoorVisualStudioImplementationDetails(runContext, frameworkHandle);

            foreach (var source in sources)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Informational, "\tSource: " + source);

                var assemblyFullPath = Path.GetFullPath(source);

                using (var environment = new ExecutionEnvironment(assemblyFullPath))
                {
                    var runner = environment.Create<ExecutionProxy>();
                    runner.RunAssembly(assemblyFullPath, new string[] { }, new VisualStudioListener(frameworkHandle, source));
                }
            }
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            var assemblyGroups = tests.GroupBy(tc => tc.Source);

            frameworkHandle.SendMessage(TestMessageLevel.Informational, "RunTests invoked with TestCase objects:");

            HandlePoorVisualStudioImplementationDetails(runContext, frameworkHandle);

            foreach (var assemblyGroup in assemblyGroups)
            {
                foreach (var test in assemblyGroup)
                {
                    var message = new StringBuilder()
                        .AppendLine("\tSource: " + test.Source)
                        .AppendLine("\tId: " + test.Id)
                        .AppendLine("\tDisplayName: " + test.DisplayName)
                        .AppendLine("\tCodeFilePath: " + test.CodeFilePath)
                        .AppendLine("\tLineNumber: " + test.LineNumber)
                        .AppendLine("\tFullyQualifiedName: " + test.FullyQualifiedName)
                        .AppendLine("\tExecutorUri: " + test.ExecutorUri.OriginalString)
                        .AppendLine()
                        .ToString();

                    frameworkHandle.SendMessage(TestMessageLevel.Informational, message);
                }

                var source = assemblyGroup.Key;

                frameworkHandle.SendMessage(TestMessageLevel.Informational, "\tSource: " + source);

                var assemblyFullPath = Path.GetFullPath(source);

                using (var environment = new ExecutionEnvironment(assemblyFullPath))
                {
                    var runner = environment.Create<ExecutionProxy>();
                    runner.RunAssembly(assemblyFullPath, new string[] { }, new VisualStudioListener(frameworkHandle, source));
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