namespace Fixie.Runner
{
    using System;
    using System.IO;
    using Contracts;
    using Execution;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Message = Contracts.Message;
    using static System.Environment;

    public interface IDesignTimeSink
    {
        void Send(string message);
        void Log(string message);
    }

    public class DesignTimeSink : LongLivedMarshalByRefObject, IDesignTimeSink
    {
        readonly BinaryWriter writer;

        public DesignTimeSink(BinaryWriter writer)
        {
            this.writer = writer;
        }

        public void Send(string message)
            => writer.Write(message);

        public void Log(string message)
            => File.AppendAllText("fixie.log", $"{DateTime.Now}: {message}{NewLine}{NewLine}");
    }

    public static class DesignTimeSinkExtensions
    {
        public static void SendWaitingCommand(this IDesignTimeSink sink)
           => Send(sink, "TestRunner.WaitingCommand");

        public static void SendTestCompleted(this IDesignTimeSink sink)
            => Send(sink, "TestRunner.TestCompleted");

        public static void SendTestFound(this IDesignTimeSink sink, Test test)
            => Send(sink, "TestDiscovery.TestFound", test);

        public static void SendTestStarted(this IDesignTimeSink sink, Test test)
            => Send(sink, "TestExecution.TestStarted", test);

        public static void SendTestResult(this IDesignTimeSink sink, TestResult testResult)
            => Send(sink, "TestExecution.TestResult", testResult);

        static void Send(IDesignTimeSink sink, string messageType, object payload = null)
        {
            sink.Send(JsonConvert.SerializeObject(new Message
            {
                MessageType = messageType,
                Payload = payload == null ? null : JToken.FromObject(payload)
            }));
        }
    }
}