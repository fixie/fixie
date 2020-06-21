namespace Fixie.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Internal;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using static AssemblyPath;

    [DefaultExecutorUri(VsTestExecutor.Id)]
    [FileExtension(".exe")]
    [FileExtension(".dll")]
    class VsTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger log, ITestCaseDiscoverySink discoverySink)
        {
            try
            {
                log.Version();

                foreach (var assemblyPath in sources)
                    DiscoverTests(log, discoverySink, assemblyPath);
            }
            catch (Exception exception)
            {
                throw new RunnerException(exception);
            }
        }

        static void DiscoverTests(IMessageLogger log, ITestCaseDiscoverySink discoverySink, string assemblyPath)
        {
            if (!IsTestAssembly(assemblyPath))
            {
                log.Info("Skipping " + assemblyPath + " because it is not a test assembly.");
                return;
            }

            log.Info("Processing " + assemblyPath);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath)!);
            var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath));
            var testAssemblyLoadContext = new TestAssemblyLoadContext(assemblyPath);
            var assembly = testAssemblyLoadContext.LoadFromAssemblyName(assemblyName);
            var listener = new DiscoveryListener(log, discoverySink, assemblyPath);
            var testAssembly = new TestAssembly(assembly, listener);

            testAssembly.Discover();
        }
    }
}
