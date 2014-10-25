using System;
using System.Collections.Generic;
using System.Text;
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

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Informational, "RunTests invoked with TestCase objects:");

            foreach (var test in tests)
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
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Informational, "RunTests invoked with string 'sources' list:");

            foreach (var source in sources)
                frameworkHandle.SendMessage(TestMessageLevel.Informational, "\tSource: " + source);
        }

        public void Cancel()
        {
        }
    }
}