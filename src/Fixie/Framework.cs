namespace Fixie
{
    using System.Diagnostics;

    public static class Framework
    {
        public static string Version
        {
            get
            {
                var framework = typeof(Framework).Assembly;
                var fileName = framework.Location;

                return framework.GetName().Name + " " + FileVersionInfo.GetVersionInfo(fileName).ProductVersion;
            }
        }
    }
}