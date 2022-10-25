namespace Fixie.Reports
{
    /// <summary>
    /// Fired when an individual test has started execution.
    /// </summary>
    public class TestStarted : IMessage
    {
        internal TestStarted(Test test)
            => Test = test.Name;

        /// <summary>
        /// The name of the test being executed.
        /// </summary>
        public string Test { get; }
    }
}