namespace Fixie.VisualStudio.TestAdapter.Wrappers
{
    using Execution;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    public class MessageLogger : LongLivedMarshalByRefObject
    {
        readonly IMessageLogger log;

        public MessageLogger(IMessageLogger log)
        {
            this.log = log;
        }

        public void Error(string error)
            => log.SendMessage(TestMessageLevel.Error, error);
    }
}