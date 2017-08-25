namespace Fixie.Execution
{
    using System;
    using System.IO;

    public class ConsoleExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
        readonly TestAppDomain domain;
        readonly RemoteAssemblyResolver assemblyResolver;
        readonly string previousWorkingDirectory;
        readonly ExecutionProxy executionProxy;

        public ConsoleExecutionEnvironment(string assemblyPath)
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

        public void DiscoverMethods(string[] arguments)
            => executionProxy.DiscoverMethods(assemblyFullPath, arguments);

        public int RunAssembly(string[] arguments)
            => executionProxy.RunAssembly(assemblyFullPath, arguments);

        public int RunMethods(string[] arguments, string[] methods)
            => executionProxy.RunMethods(assemblyFullPath, arguments, methods);

        public void Dispose()
        {
            executionProxy.Dispose();
            assemblyResolver.Dispose();
            domain.Dispose();
            Directory.SetCurrentDirectory(previousWorkingDirectory);
        }
    }
}