using System;
using System.Reflection;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyCompleted : IMessage
    {
        public AssemblyCompleted(Assembly assembly)
        {
            var assemblyName = assembly.GetName();

            Name = assemblyName.Name;
            FullName = assemblyName.FullName;
            Version = assemblyName.Version.ToString();
            Location = assembly.Location;
        }

        public string Name { get; }
        public string FullName { get; }
        public string Version { get; }
        public string Location { get; }
    }
}