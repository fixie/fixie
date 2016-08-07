namespace Fixie.Execution
{
    using System.Reflection;

    public class AssemblyCompleted : Message
    {
        public AssemblyCompleted(Assembly assembly)
        {
            Name = assembly.GetName().Name;
            Location = assembly.Location;
        }

        public string Name { get; }
        public string Location { get; }
    }
}