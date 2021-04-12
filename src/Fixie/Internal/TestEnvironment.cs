namespace Fixie.Internal
{
    using System.IO;
    using System.Reflection;

    class TestEnvironment
    {
        public TestEnvironment(Assembly assembly, TextWriter console, string rootDirectory)
            : this(assembly, new string[] {}, console, rootDirectory) { }

        public TestEnvironment(Assembly assembly, string[] customArguments, TextWriter console, string rootDirectory)
        {
            Assembly = assembly;
            CustomArguments = customArguments;
            Console = console;
            RootDirectory = rootDirectory;
        }

        public Assembly Assembly { get; }
        public string[] CustomArguments { get; }
        public TextWriter Console { get; }
        public string RootDirectory { get; }
    }
}
