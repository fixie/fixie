using Fixie.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;

namespace Fixie.Execution
{
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
        }

        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(Options options)
        {
            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.DiscoverTestMethodGroups(assemblyFullPath, options);
        }

        public AssemblyResult RunAssembly<TListenerFactory>(Options options, IExecutionSink executionSink) where TListenerFactory : IListenerFactory
        {
            AssertIsLongLivedMarshalByRefObject(executionSink);

            var listenerFactoryAssemblyFullPath = typeof(TListenerFactory).Assembly.Location;
            var listenerFactoryType = typeof(TListenerFactory).FullName;

            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.RunAssembly(assemblyFullPath, listenerFactoryAssemblyFullPath, listenerFactoryType, options, executionSink);
        }

        public AssemblyResult RunMethods<TListenerFactory>(Options options, IExecutionSink executionSink, MethodGroup[] methodGroups) where TListenerFactory : IListenerFactory
        {
            AssertIsLongLivedMarshalByRefObject(executionSink);

            var listenerFactoryAssemblyFullPath = typeof(TListenerFactory).Assembly.Location;
            var listenerFactoryType = typeof(TListenerFactory).FullName;

            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.RunMethods(assemblyFullPath, listenerFactoryAssemblyFullPath, listenerFactoryType, options, executionSink, methodGroups);
        }

        [Obsolete]
        public AssemblyResult RunAssembly(Options options, Listener listener)
        {
            AssertIsLongLivedMarshalByRefObject(listener);

            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.RunAssembly(assemblyFullPath, options, listener);
        }

        [Obsolete]
        public AssemblyResult RunMethods(Options options, Listener listener, MethodGroup[] methodGroups)
        {
            AssertIsLongLivedMarshalByRefObject(listener);

            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.RunMethods(assemblyFullPath, options, listener, methodGroups);
        }

        static void AssertIsLongLivedMarshalByRefObject(object o)
        {
            if (o == null) return;
            if (o is LongLivedMarshalByRefObject) return;
            var type = o.GetType();
            var message = string.Format("Type '{0}' in Assembly '{1}' must inherit from '{2}'.",
                                        type.FullName,
                                        type.Assembly,
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
    }
}