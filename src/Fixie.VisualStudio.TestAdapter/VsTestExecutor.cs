using System;
using System.Collections.Generic;
using System.Linq;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.VisualStudio.TestAdapter
{
    [ExtensionUri(Id)]
    public class VsTestExecutor : ITestExecutor
    {
        public const string Id = "executor://Fixie.VisualStudio";
        public static Uri Uri = new Uri(Id);

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            IMessageLogger log = frameworkHandle;

            log.Version();

            HandlePoorVisualStudioImplementationDetails(runContext, frameworkHandle);

            foreach (var assemblyPath in sources)
            {
                log.Info("Processing " + assemblyPath);

                try
                {
                    var listener = new VisualStudioListener(frameworkHandle, assemblyPath);

                    using (var environment = new ExecutionEnvironment(assemblyPath))
                    {
                        environment.RunAssembly(new Lookup(), listener);
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
            IMessageLogger log = frameworkHandle;

            log.Version();

            HandlePoorVisualStudioImplementationDetails(runContext, frameworkHandle);

            var assemblyGroups = tests.GroupBy(tc => tc.Source);

            foreach (var assemblyGroup in assemblyGroups)
            {
                var assemblyPath = assemblyGroup.Key;

                log.Info("Processing " + assemblyPath);

                try
                {
                    var methodGroups = assemblyGroup.Select(x => new MethodGroup(x.FullyQualifiedName)).ToArray();

                    var listener = new VisualStudioListener(frameworkHandle, assemblyPath);

                    using (var environment = new ExecutionEnvironment(assemblyPath))
                    {
                        environment.RunMethods(new Lookup(), listener, methodGroups);
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