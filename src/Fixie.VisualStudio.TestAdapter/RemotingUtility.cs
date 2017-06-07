namespace Fixie.VisualStudio.TestAdapter
{
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
#if NET452
            foreach (var chan in System.Runtime.Remoting.Channels.ChannelServices.RegisteredChannels)
                System.Runtime.Remoting.Channels.ChannelServices.UnregisterChannel(chan);
#endif
        }
    }
}
