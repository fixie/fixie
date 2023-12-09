using System;
using Fixie.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.TestAdapter;

static class LoggingExtensions
{
    public static void Info(this IMessageLogger logger, string message)
        => logger.SendMessage(TestMessageLevel.Informational, message);

    public static void Error(this IMessageLogger logger, string message)
        => logger.SendMessage(TestMessageLevel.Error, message);

    public static void Error(this IMessageLogger logger, Exception exception)
        => logger.SendMessage(TestMessageLevel.Error, exception.ToString());

    public static void Version(this IMessageLogger logger)
        => logger.Info(Framework.Version);
}