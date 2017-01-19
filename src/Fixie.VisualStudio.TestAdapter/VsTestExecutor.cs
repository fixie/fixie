namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Execution;
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

                        using (var executionRecorder = new ExecutionRecorder(frameworkHandle, assemblyPath))
                        using (var environment = new ExecutionEnvironment(assemblyPath))
                        {
                            environment.Subscribe<VisualStudioExecutionListener>(executionRecorder);
                            environment.RunAssembly(new string[] {}, new string[] { });
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

                        var methodGroups = assemblyGroup.Select(x => new MethodGroup(x.FullyQualifiedName)).ToArray();

                        using (var executionRecorder = new ExecutionRecorder(frameworkHandle, assemblyPath))
                        using (var environment = new ExecutionEnvironment(assemblyPath))
                        {
                            environment.Subscribe<VisualStudioExecutionListener>(executionRecorder);
                            environment.RunMethods(new string[] {}, new string[] {}, methodGroups);
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
            RemotingUtility.CleanUpRegisteredChannels();

            if (runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;
        }

        static bool AssemblyDirectoryContainsFixie(string assemblyPath)
        {
            return File.Exists(Path.Combine(Path.GetDirectoryName(assemblyPath), "Fixie.dll"));
        }
    }
}