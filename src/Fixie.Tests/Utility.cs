namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Fixie.Internal;

    public static class Utility
    {
        public static string FullName<T>()
        {
            return typeof(T).FullName ??
                   throw new Exception($"Expected type {typeof(T).Name} to have a non-null FullName.");
        }

        public static string At<T>(string method, [CallerFilePath] string path = default!)
            => $"   at {FullName<T>().Replace("+", ".")}.{method} in {path}:line #";

        public static string[] For<TSampleTestClass>(params string[] entries)
            => entries.Select(x => FullName<TSampleTestClass>() + x).ToArray();

        public static string PathToThisFile([CallerFilePath] string path = default!)
            => path;

        public static IEnumerable<string> Run<TSampleTestClass>()
            => Run<TSampleTestClass, DefaultExecution>();

        public static IEnumerable<string> Run<TSampleTestClass, TExecution>() where TExecution : Execution, new()
            => Run<TSampleTestClass>(new TExecution());

        public static IEnumerable<string> Run<TSampleTestClass>(Execution execution)
            => Run(typeof(TSampleTestClass), execution);

        public static IEnumerable<string> Run<TExecution>(Type testClass) where TExecution : Execution, new()
            => Run(testClass, new TExecution());

        public static IEnumerable<string> Run(Type testClass, Execution execution)
        {
            var listener = new StubListener();
            var discovery = new SelfTestDiscovery();
            Run(listener, discovery, execution, testClass).GetAwaiter().GetResult();
            return listener.Entries;
        }

        public static void Discover(Listener listener, Discovery discovery, params Type[] candidateTypes)
        {
            if (candidateTypes.Length == 0)
                throw new InvalidOperationException("At least one type must be specified.");

            new Runner(candidateTypes[0].Assembly, listener).Discover(candidateTypes, discovery);
        }

        public static Task Run(Listener listener, Discovery discovery, Execution execution, params Type[] candidateTypes)
        {
            if (candidateTypes.Length == 0)
                throw new InvalidOperationException("At least one type must be specified.");

            var runner = new Runner(candidateTypes[0].Assembly, listener);

            return runner.Run(candidateTypes, discovery, execution, ImmutableHashSet<string>.Empty);
        }

        public static IEnumerable<object?[]> UsingInputAttributes(MethodInfo method)
            => method
                .GetCustomAttributes<InputAttribute>(true)
                .Select(input => input.Parameters);
    }
}
