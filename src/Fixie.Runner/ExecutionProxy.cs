namespace Fixie.Runner
{
    using System.Collections.Generic;
    using System.Reflection;
    using Cli;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        public int Run(string assemblyFullPath, IReadOnlyList<string> runnerArguments, IReadOnlyList<string> conventionArguments)
        {
            var options = CommandLine.Parse<Options>(runnerArguments);

            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));

            return Runner(options).Run(assemblyFullPath, assembly, options, conventionArguments);
        }

        static RunnerBase Runner(Options options)
        {
            if (options.DesignTime)
                return new DesignTimeRunner();

            return new ConsoleRunner();
        }
    }
}