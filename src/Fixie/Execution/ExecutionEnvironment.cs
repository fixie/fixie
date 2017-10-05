using Fixie.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;

namespace Fixie.Execution
{
    using System.Reflection;

    public class ExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
        readonly AppDomain appDomain;
        readonly string previousWorkingDirectory;

        public ExecutionEnvironment(string assemblyPath)
        {
            assemblyFullPath = Path.GetFullPath(assemblyPath);
            appDomain = CreateAppDomain(assemblyFullPath);

            previousWorkingDirectory = Directory.GetCurrentDirectory();
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
            Directory.SetCurrentDirectory(assemblyDirectory);
            var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFullPath);
            Create<AppDomainFixer>(assembly);
        }

        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(Options options)
        {
            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.DiscoverTestMethodGroups(assemblyFullPath, options);
        }

        public AssemblyResult RunAssembly(Options options, Listener listener)
        {
            AssertIsLongLivedMarshalByRefObject(listener);

            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.RunAssembly(assemblyFullPath, options, listener);
        }

        public AssemblyResult RunMethods(Options options, Listener listener, MethodGroup[] methodGroups)
        {
            AssertIsLongLivedMarshalByRefObject(listener);

            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.RunMethods(assemblyFullPath, options, listener, methodGroups);
        }

        static void AssertIsLongLivedMarshalByRefObject(Listener listener)
        {
            if (listener is LongLivedMarshalByRefObject) return;
            var listenerType = listener.GetType();
            var message = string.Format("Type '{0}' in Assembly '{1}' must inherit from '{2}'.",
                                        listenerType.FullName,
                                        listenerType.Assembly,
                                        typeof(LongLivedMarshalByRefObject).FullName);
            throw new Exception(message);
        }

        T Create<T>(params object[] args) where T : MarshalByRefObject
        {
            return (T)appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, false, 0, null, args, null, null);
        }

        public void Dispose()
        {
            AppDomain.Unload(appDomain);
            Directory.SetCurrentDirectory(previousWorkingDirectory);
        }

        static AppDomain CreateAppDomain(string assemblyFullPath)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(assemblyFullPath),
                ApplicationName = Guid.NewGuid().ToString(),
                ConfigurationFile = GetOptionalConfigFullPath(assemblyFullPath)
            };

            return AppDomain.CreateDomain(setup.ApplicationName, null, setup, new PermissionSet(PermissionState.Unrestricted));
        }

        static string GetOptionalConfigFullPath(string assemblyFullPath)
        {
            var configFullPath = assemblyFullPath + ".config";

            return File.Exists(configFullPath) ? configFullPath : null;
        }

        class AppDomainFixer : MarshalByRefObject
        {
            public AppDomainFixer(Assembly entryAssembly)
            {
                // See: http://dejanstojanovic.net/aspnet/2015/january/set-entry-assembly-in-unit-testing-methods/
                AppDomainManager manager = new AppDomainManager();
                FieldInfo entryAssemblyfield = manager.GetType().GetField("m_entryAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
                entryAssemblyfield?.SetValue(manager, entryAssembly);

                AppDomain domain = AppDomain.CurrentDomain;
                FieldInfo domainManagerField = domain.GetType().GetField("_domainManager", BindingFlags.Instance | BindingFlags.NonPublic);
                domainManagerField?.SetValue(domain, manager);
                Console.WriteLine($"Assembly full name: {Assembly.GetEntryAssembly()?.FullName}");
            }
        }
    }

}