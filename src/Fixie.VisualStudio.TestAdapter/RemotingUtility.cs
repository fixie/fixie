namespace Fixie.VisualStudio.TestAdapter
{
    using System.Runtime.Remoting.Channels;

    public static class RemotingUtility
    {
        /// <summary>
        /// MSTest has a history of registering remoting channels when it runs, without cleaning them up.
        /// If this happens, then MarshalByRefObjects fail to work properly.
        /// 
        /// The NUnit and xUnit runners perform the same cleanup operations in their own Visual Studio Test Adapters.
        /// See http://xunit.codeplex.com/workitem/9749
        /// </summary>
        public static void CleanUpRegisteredChannels()
        {
            foreach (var chan in ChannelServices.RegisteredChannels)
                ChannelServices.UnregisterChannel(chan);
        }
    }
}