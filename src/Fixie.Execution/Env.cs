namespace Fixie.Execution
{
#if NET45
    using System;

    static class Env
    {
        public static string Version
            => Environment.Version.ToString();

        public static string OSVersion
            => Environment.OSVersion.ToString();

        public static string OSVersionPlatform
            => Environment.OSVersion.Platform.ToString();

        public static string MachineName
            => Environment.MachineName;

        public static string UserName
            => Environment.UserName;

        public static string UserDomainName
            => Environment.UserDomainName;

        public static string ConfigurationFile
            => AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
    }
#else
    using System.Runtime.InteropServices;

    static class Env
    {
        public static string Version
            => RuntimeInformation.FrameworkDescription;

        public static string OSVersion
            => RuntimeInformation.OSDescription;

        public static string OSVersionPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return OSPlatform.Windows.ToString();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return OSPlatform.Linux.ToString();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return OSPlatform.OSX.ToString();

                return "Unknown Platform";
            }
        }

        public static string MachineName
            => "Unknown MachineName";

        public static string UserName
            => "Unknown UserName";

        public static string UserDomainName
            => "Unknown UserDomainName";

        public static string ConfigurationFile
            => "Unknown Configuration File";
    }
#endif
}
