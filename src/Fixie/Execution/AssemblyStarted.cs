using System.Reflection;

namespace Fixie.Execution
{
    public class AssemblyStarted
    {
        public AssemblyStarted(Assembly assembly)
        {
            Assembly = assembly;
        }

        public Assembly Assembly { get; }
    }
}