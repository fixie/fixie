using System.Diagnostics;
using System.Reflection;

namespace Fixie
{
    public static class Framework
    {
        public static string Version
        {
            get
            {
                var framework = Assembly.GetExecutingAssembly();
                var fileName = framework.Location;

                return framework.GetName().Name + " " + FileVersionInfo.GetVersionInfo(fileName).ProductVersion;
            }
        }
    }
}