namespace Fixie.Reports
{
    public class TestDiscovered : IMessage
    {
        internal TestDiscovered(string test)
            => Test = test;

        public string Test { get; }
    }
}