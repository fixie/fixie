namespace Fixie.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Internal;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using static AssemblyPath;

    [ExtensionUri(Id)]
    class VsTestExecutor : ITestExecutor
    {
        public const string Id = "executor://fixie.testadapter/";
        public static readonly Uri Uri = new Uri(Id);

        /// <summary>
        /// Called when running all tests.
        /// </summary>
        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            try
            {
                IMessageLogger log = frameworkHandle;

                log.Version();

                HandlePoorVsTestImplementationDetails(runContext, frameworkHandle);

                foreach (var assemblyPath in sources)
                    RunTests(log, frameworkHandle, assemblyPath, runner => runner.Run());
            }
            catch (Exception exception)
            {
                throw new RunnerException(exception);
            }
        }

        /// <summary>
        /// Called when running selected tests.
        /// </summary>
        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            try
            {
                IMessageLogger log = frameworkHandle;

                log.Version();

                HandlePoorVsTestImplementationDetails(runContext, frameworkHandle);

                var assemblyGroups = tests.GroupBy(tc => tc.Source);

                foreach (var assemblyGroup in assemblyGroups)
                {
                    var assemblyPath = assemblyGroup.Key;

                    RunTests(log, frameworkHandle, assemblyPath, runner =>
                    {
                        runner.Run(assemblyGroup.Select(x => x.FullyQualifiedName).ToImmutableHashSet())
                            .GetAwaiter().GetResult();
                    });
                }
            }
            catch (Exception exception)
            {
                throw new RunnerException(exception);
            }
        }

        public void Cancel() { }

        static void RunTests(IMessageLogger log, IFrameworkHandle frameworkHandle, string assemblyPath, Action<Runner> run)
        {
            if (!IsTestAssembly(assemblyPath))
            {
                log.Info("Skipping " + assemblyPath + " because it is not a test assembly.");
                return;
            }

            log.Info("Processing " + assemblyPath);

            Directory.SetCurrentDirectory(FolderPath(assemblyPath));
            var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath));
            var testAssemblyLoadContext = new TestAssemblyLoadContext(assemblyPath);
            var assembly = testAssemblyLoadContext.LoadFromAssemblyName(assemblyName);
            var listener = new ExecutionListener(frameworkHandle, assemblyPath);
            var runner = new Runner(assembly, listener);

            run(runner);
        }

        static void HandlePoorVsTestImplementationDetails(IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;
        }
    }
}