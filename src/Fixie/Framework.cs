namespace Fixie
{
    using System.Diagnostics;
    using System.Reflection;

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