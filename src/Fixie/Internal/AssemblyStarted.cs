namespace Fixie.Internal
{
    using System.Reflection;

    public class AssemblyStarted : Message
    {
        public AssemblyStarted(Assembly assembly)
        {
            Assembly = assembly;
        }

        public Assembly Assembly { get; }
    }
}