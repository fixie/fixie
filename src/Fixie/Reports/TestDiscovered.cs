namespace Fixie.Reports
{
    /// <summary>
    /// Fired once per discovered test.
    /// </summary>
    public class TestDiscovered : IMessage
    {
        internal TestDiscovered(string test)
            => Test = test;

        /// <summary>
        /// The name of the discovered test.
        /// </summary>
        public string Test { get; }
    }
}