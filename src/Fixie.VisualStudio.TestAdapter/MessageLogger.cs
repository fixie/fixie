namespace Fixie.VisualStudio.TestAdapter
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    public class MessageLogger
    {
        readonly IMessageLogger log;

        public MessageLogger(IMessageLogger log)
        {
            this.log = log;
        }

        public void SendMessage(TestMessageLevel testMessageLevel, string message)
        {
            log.SendMessage(testMessageLevel, message);
        }
    }
}