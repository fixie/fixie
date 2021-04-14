namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public class TestContext
    {
        public TestContext(Assembly assembly, TextWriter console, string rootDirectory)
            : this(assembly, console, rootDirectory, Array.Empty<string>()) { }

        public TestContext(Assembly assembly, TextWriter console, string rootDirectory,
            IReadOnlyList<string> customArguments)
        {
            Assembly = assembly;
            CustomArguments = customArguments;
            Console = console;
            RootDirectory = rootDirectory;
        }

        public Assembly Assembly { get; }
        public IReadOnlyList<string> CustomArguments { get; }
        public TextWriter Console { get; }
        public string RootDirectory { get; }
    }
}
