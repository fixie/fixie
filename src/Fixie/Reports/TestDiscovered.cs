namespace Fixie.Reports
{
    public class TestDiscovered : Message
    {
        internal TestDiscovered(Test test)
            => Test = test;

        public Test Test { get; }
    }
}