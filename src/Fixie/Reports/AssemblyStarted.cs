namespace Fixie.Reports
{
    using System.Reflection;

    public class AssemblyStarted : Message
    {
        internal AssemblyStarted(Assembly assembly)
        {
            Assembly = assembly;
        }

        public Assembly Assembly { get; }
    }
}