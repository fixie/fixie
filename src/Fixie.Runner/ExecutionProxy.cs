namespace Fixie.Runner
{
    using System.Collections.Generic;
    using Cli;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        public int Run(string assemblyFullPath, IReadOnlyList<string> runnerArguments, IReadOnlyList<string> conventionArguments)
        {
            var options = CommandLine.Parse<Options>(runnerArguments);

            return Runner(options).Run(assemblyFullPath, options, conventionArguments);
        }

        static RunnerBase Runner(Options options)
        {
            if (options.DesignTime)
                return new DesignTimeRunner();

            return new ConsoleRunner();
        }
    }
}