namespace Fixie.Execution
{
    using System;

    class ConsoleTestAppDomain
    {
        public T CreateFrom<T>() where T : LongLivedMarshalByRefObject, new()
            => new T();

        public T Create<T>(params object[] arguments) where T : LongLivedMarshalByRefObject
            => (T)Activator.CreateInstance(typeof(T), arguments);
    }
}
