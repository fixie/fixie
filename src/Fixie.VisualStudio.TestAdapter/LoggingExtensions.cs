using System;
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
    }
}