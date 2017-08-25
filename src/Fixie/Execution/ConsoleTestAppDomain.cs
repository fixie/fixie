namespace Fixie.Execution
{
#if NET452
    using System;

    class ConsoleTestAppDomain : IDisposable
    {
        public ConsoleTestAppDomain(string assemblyFullPath) { }

        public T CreateFrom<T>() where T : LongLivedMarshalByRefObject, new()
            => new T();

        public T Create<T>(params object[] arguments) where T : LongLivedMarshalByRefObject
            => (T)Activator.CreateInstance(typeof(T), arguments);

        public void Dispose() { }
    }
#else
    using System;

    class ConsoleTestAppDomain : IDisposable
    {
        public ConsoleTestAppDomain(string assemblyFullPath) { }

        public T CreateFrom<T>() where T : LongLivedMarshalByRefObject, new()
            => new T();

        public T Create<T>(params object[] arguments) where T : LongLivedMarshalByRefObject
            => (T)Activator.CreateInstance(typeof(T), arguments);

        public void Dispose() { }
    }
#endif
}
