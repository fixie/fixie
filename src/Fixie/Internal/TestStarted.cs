namespace Fixie.Internal
{
    public class TestStarted : Message
    {
        public TestStarted(Test test)
            => Test = test;

        public Test Test { get; }
    }
}