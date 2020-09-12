namespace Fixie
{
    using System.Reflection;

    public class TestAssembly
    {
        internal TestAssembly(Assembly assembly)
        {
            Assembly = assembly;
        }

        public Assembly Assembly { get; }
    }
}