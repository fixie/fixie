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
        readonly RemoteAssemblyResolver assemblyResolver;
        readonly ExecutionProxy executionProxy;

        public ExecutionEnvironment(string assemblyPath)
        {
            assemblyFullPath = Path.GetFullPath(assemblyPath);
            appDomain = CreateAppDomain(assemblyFullPath);

            previousWorkingDirectory = Directory.GetCurrentDirectory();
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
            Directory.SetCurrentDirectory(assemblyDirectory);

            assemblyResolver = Create<RemoteAssemblyResolver>();
            executionProxy = Create<ExecutionProxy>();
        }

        public void ResolveAssemblyContaining<T>()
        {
            assemblyResolver.AddAssemblyLocation(typeof(T).Assembly.Location);
        }

        public void Subscribe<THandler>(params object[] handlerArgs)
        {
            foreach (var arg in handlerArgs)
                AssertSafeForAppDomainCommunication(arg);

            var handlerAssemblyFullPath = typeof(THandler).Assembly.Location;
            var handlerType = typeof(THandler).FullName;

            executionProxy.Subscribe(handlerAssemblyFullPath, handlerType, handlerArgs);
        }

        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(Options options)
        {
            return executionProxy.DiscoverTestMethodGroups(assemblyFullPath, options);
        }

        public AssemblyResult RunAssembly(Options options)
        {
            return executionProxy.RunAssembly(assemblyFullPath, options);
        }

        public AssemblyResult RunMethods(Options options, MethodGroup[] methodGroups)
        {
            return executionProxy.RunMethods(assemblyFullPath, options, methodGroups);
        }

        static void AssertSafeForAppDomainCommunication(object o)
        {
            if (o == null) return;
            if (o is LongLivedMarshalByRefObject) return;
            if (o.GetType().Has<SerializableAttribute>()) return;

            var type = o.GetType();
            var message = $"Type '{type.FullName}' in Assembly '{type.Assembly}' must either be [Serialiable] or inherit from '{typeof(LongLivedMarshalByRefObject).FullName}'.";
            throw new Exception(message);
        }

        T Create<T>(params object[] args) where T : LongLivedMarshalByRefObject
        {
            return (T)appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, false, 0, null, args, null, null);
        }

        public void Dispose()
        {
            executionProxy.Dispose();
            assemblyResolver.Dispose();
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