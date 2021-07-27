namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public class TestEnvironment
    {
        internal TestEnvironment(Assembly assembly, TextWriter console, string rootPath)
            : this(assembly, console, rootPath, Array.Empty<string>()) { }

        internal TestEnvironment(Assembly assembly, TextWriter console, string rootPath,
            IReadOnlyList<string> customArguments)
        {
            Assembly = assembly;
            CustomArguments = customArguments;
            Console = console;
            RootPath = rootPath;
        }

        /// <summary>
        /// The test assembly being executed.
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Optional custom command line arguments provided to the test runner.
        /// </summary>
        public IReadOnlyList<string> CustomArguments { get; }

        /// <summary>
        /// The standard output stream. Use this to reliably write to the test
        /// runner's original standard output stream, even if tests or the
        /// system under test have redirected System.Console.Out.
        /// </summary>
        public TextWriter Console { get; }
        
        /// <summary>
        /// The absolute path to the directory containing the test assembly.
        /// </summary>
        public string RootPath { get; }
    }
}
