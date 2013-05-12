namespace Fixie
{
    public interface FixtureCommand
    {
        void Execute(object fixtureInstance, ExceptionList exceptions);
    }
}