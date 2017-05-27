namespace Fixie.Execution
{
    using System;
    using System.IO;

    public class ExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
        readonly TestAppDomain domain;
        readonly RemoteAssemblyResolver assemblyResolver;
        readonly string previousWorkingDirectory;
        readonly ExecutionProxy executionProxy;

        public ExecutionEnvironment(string assemblyPath)
        {
            assemblyFullPath = Path.GetFullPath(assemblyPath);
            domain = new TestAppDomain(assemblyFullPath);

            previousWorkingDirectory = Directory.GetCurrentDirectory();
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
            Directory.SetCurrentDirectory(assemblyDirectory);

            assemblyResolver = domain.CreateFrom<RemoteAssemblyResolver>();
            assemblyResolver.RegisterAssemblyLocation(typeof(ExecutionProxy).Assembly().Location);

            executionProxy = domain.Create<ExecutionProxy>(previousWorkingDirectory);
        }

        public void Subscribe<TListener>(params object[] listenerArguments) where TListener : Listener
        {
            assemblyResolver.RegisterAssemblyLocation(typeof(TListener).Assembly().Location);
            executionProxy.Subscribe<TListener>(listenerArguments);
        }

        public void DiscoverMethods(string[] runnerArguments, string[] conventionArguments)
            => executionProxy.DiscoverMethods(assemblyFullPath, runnerArguments, conventionArguments);

        public int RunAssembly(string[] runnerArguments, string[] conventionArguments)
            => executionProxy.RunAssembly(assemblyFullPath, runnerArguments, conventionArguments);

        public void RunMethods(string[] runnerArguments, string[] conventionArguments, string[] methods)
            => executionProxy.RunMethods(assemblyFullPath, runnerArguments, conventionArguments, methods);

        public void Dispose()
        {
            executionProxy.Dispose();
            assemblyResolver.Dispose();
            domain.Dispose();
            Directory.SetCurrentDirectory(previousWorkingDirectory);
        }
    }
}