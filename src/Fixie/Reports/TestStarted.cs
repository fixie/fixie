namespace Fixie.Reports;

public class TestStarted : IMessage
{
    internal TestStarted(Test test)
        => Test = test.Name;

    public string Test { get; }
}