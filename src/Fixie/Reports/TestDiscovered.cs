namespace Fixie.Reports
{
    public class TestDiscovered : Message
    {
        internal TestDiscovered(TestName test)
            => Test = test;

        public TestName Test { get; }
    }
}