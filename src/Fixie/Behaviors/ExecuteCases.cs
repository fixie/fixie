namespace Fixie.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        public void Execute(Fixture fixture)
        {
            foreach (var @case in fixture.Cases)
                fixture.CaseExecutionBehavior.Execute(@case, fixture.Instance);
        }
    }
}