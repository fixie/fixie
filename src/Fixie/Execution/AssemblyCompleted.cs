namespace Fixie.Execution
{
    using System.Reflection;

    public class AssemblyCompleted : Message
    {
        public AssemblyCompleted(Assembly assembly, ExecutionSummary summary)
        {
            Assembly = assembly;
            Summary = summary;
        }

        public Assembly Assembly { get; }
        public ExecutionSummary Summary { get; }
    }
}