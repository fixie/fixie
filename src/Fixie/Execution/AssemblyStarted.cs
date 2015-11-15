using System.Reflection;

namespace Fixie.Execution
{
    public class AssemblyStarted : IMessage
    {
        public AssemblyStarted(Assembly assembly)
        {
            Assembly = assembly;
        }

        public Assembly Assembly { get; }
    }
}