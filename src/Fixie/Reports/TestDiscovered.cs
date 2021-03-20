namespace Fixie.Reports
{
    public class TestDiscovered : Message
    {
        internal TestDiscovered(string test)
            => Test = test;

        public string Test { get; }
    }
}