namespace Fixie.Internal
{
    using System.IO;
    using System.Reflection;

    class TestEnvironment
    {
        public TestEnvironment(Assembly assembly, TextWriter console)
            : this(assembly, new string[] {}, console) { }

        public TestEnvironment(Assembly assembly, string[] customArguments, TextWriter console)
        {
            Assembly = assembly;
            CustomArguments = customArguments;
            Console = console;
        }

        public Assembly Assembly { get; }
        public string[] CustomArguments { get; }
        public TextWriter Console { get; }
    }
}
