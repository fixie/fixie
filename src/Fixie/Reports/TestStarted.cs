namespace Fixie.Reports
{
    public class TestStarted : Message
    {
        internal TestStarted(Test test)
            => Test = test.Name;

        public string Test { get; }
    }
}