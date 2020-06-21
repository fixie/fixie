namespace Fixie.Internal
{
    public class TestDiscovered : Message
    {
        public TestDiscovered(Test test)
            => Test = test;

        public Test Test { get; }
    }
}