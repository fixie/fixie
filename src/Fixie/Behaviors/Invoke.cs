namespace Fixie.Behaviors
{
    public class Invoke : CaseBehavior
    {
        public void Execute(Case @case, object instance)
        {
            @case.Execute(instance);
        }
    }
}