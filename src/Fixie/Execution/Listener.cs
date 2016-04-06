namespace Fixie.Execution
{
    public interface Listener:
        Handler<AssemblyInfo>,
        Handler<SkipResult>,
        Handler<PassResult>,
        Handler<FailResult>,
        Handler<AssemblyCompleted>
    {
    }
}