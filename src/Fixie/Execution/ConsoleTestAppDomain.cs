namespace Fixie.Execution
{
#if NET452
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    class ConsoleTestAppDomain : IDisposable
    {
        readonly AppDomain appDomain;

        public ConsoleTestAppDomain(string assemblyFullPath)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(assemblyFullPath),
                ApplicationName = Guid.NewGuid().ToString(),
                ConfigurationFile = GetOptionalConfigFullPath(assemblyFullPath)
            };

            appDomain = AppDomain.CreateDomain(setup.ApplicationName, null, setup, new PermissionSet(PermissionState.Unrestricted));
        }

        public T CreateFrom<T>() where T : LongLivedMarshalByRefObject, new()
        {
            return (T)appDomain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName, false, 0, null, null, null, null);
        }

        public T Create<T>(params object[] arguments) where T : LongLivedMarshalByRefObject
        {
            return (T)appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, false, 0, null, arguments, null, null);
        }

        static string GetOptionalConfigFullPath(string assemblyFullPath)
        {
            var configFullPath = assemblyFullPath + ".config";

            return File.Exists(configFullPath) ? configFullPath : null;
        }

        public void Dispose()
        {
            AppDomain.Unload(appDomain);
        }
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
