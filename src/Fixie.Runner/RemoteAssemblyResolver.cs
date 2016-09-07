namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    public class RemoteAssemblyResolver : LongLivedMarshalByRefObject
    {
        readonly List<string> allowedAssemblyLocations = new List<string>();

        public void RegisterAssemblyLocation(string assemblyLocation)
        {
            if (!allowedAssemblyLocations.Contains(assemblyLocation))
                allowedAssemblyLocations.Add(assemblyLocation);
        }

        public RemoteAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        public override void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= Resolve;
            base.Dispose();
        }

        Assembly Resolve(object sender, ResolveEventArgs args)
        {
            var pathTailWithoutExtension = Path.DirectorySeparatorChar + new AssemblyName(args.Name).Name;

            foreach (var location in allowedAssemblyLocations)
            {
                try
                {
                    if (location.EndsWith(pathTailWithoutExtension + ".dll") && File.Exists(location))
                        return LoadAssembly(location);

                    if (location.EndsWith(pathTailWithoutExtension + ".exe") && File.Exists(location))
                        return LoadAssembly(location);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(typeof(RemoteAssemblyResolver).FullName + " failed to load assembly at " + location + ": " + ex);
                }
            }

            return null;
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }
    }
}