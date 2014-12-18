using System;
using System.Collections.Generic;
using System.IO;
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
                try
                {
                    if (AssemblyDirectoryContainsFixie(assemblyPath))
                    {
                        log.Info("Processing " + assemblyPath);

                        var listener = new VisualStudioListener(frameworkHandle, assemblyPath);

                        using (var environment = new ExecutionEnvironment(assemblyPath))
                        {
                            var vsOutputWindow = new VsOutputWindow(log);
                            environment.Create<ConsoleRedirectionProxy>().RedirectConsole(vsOutputWindow);
                            environment.RunAssembly(new Options(), listener);
                            vsOutputWindow.Flush();
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

                        var listener = new VisualStudioListener(frameworkHandle, assemblyPath);

                        using (var environment = new ExecutionEnvironment(assemblyPath))
                        {
                            var vsOutputWindow = new VsOutputWindow(log);
                            environment.Create<ConsoleRedirectionProxy>().RedirectConsole(vsOutputWindow);
                            environment.RunMethods(new Options(), listener, methodGroups);
                            vsOutputWindow.Flush();
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