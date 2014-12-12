using System;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.VisualStudio.TestAdapter
{
    public static class LoggingExtensions
    {
        public static void Info(this IMessageLogger logger, string message)
        {
            logger.SendMessage(TestMessageLevel.Informational, message);
        }

        public static void Error(this IMessageLogger logger, Exception exception)
        {
            logger.SendMessage(TestMessageLevel.Error, exception.ToString());
        }

        public static void Version(this IMessageLogger logger)
        {
            var assemblyName = typeof(ExecutionEnvironment).Assembly.GetName();
            var name = assemblyName.Name;
            var version = assemblyName.Version;

            logger.Info(name + " " + version);
        }
    }
}