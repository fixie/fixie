namespace Fixie.Execution
{
    using System;
    using System.IO;

    public class ConsoleExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
        readonly RemoteAssemblyResolver assemblyResolver;
        readonly string previousWorkingDirectory;
        readonly ExecutionProxy executionProxy;

        public ConsoleExecutionEnvironment(string assemblyPath)
        {
            assemblyFullPath = Path.GetFullPath(assemblyPath);

            previousWorkingDirectory = Directory.GetCurrentDirectory();
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
            Directory.SetCurrentDirectory(assemblyDirectory);

            assemblyResolver = new RemoteAssemblyResolver();

            executionProxy = new ExecutionProxy(previousWorkingDirectory);
        }

        public int RunAssembly(string[] arguments)
            => executionProxy.RunAssembly(assemblyFullPath, arguments);

        public void Dispose()
        {
            executionProxy.Dispose();
            assemblyResolver.Dispose();
            Directory.SetCurrentDirectory(previousWorkingDirectory);
        }
    }
}