namespace Fixie.Execution
{
    using System;
    using System.IO;

    public class ConsoleExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
        readonly string previousWorkingDirectory;
        readonly ExecutionProxy executionProxy;

        public ConsoleExecutionEnvironment(string assemblyPath)
        {
            assemblyFullPath = Path.GetFullPath(assemblyPath);

            previousWorkingDirectory = Directory.GetCurrentDirectory();
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
            Directory.SetCurrentDirectory(assemblyDirectory);

            executionProxy = new ExecutionProxy(previousWorkingDirectory);
        }

        public int RunAssembly(string[] arguments)
            => executionProxy.RunAssembly(assemblyFullPath, arguments);

        public void Dispose()
        {
            executionProxy.Dispose();
            Directory.SetCurrentDirectory(previousWorkingDirectory);
        }
    }
}