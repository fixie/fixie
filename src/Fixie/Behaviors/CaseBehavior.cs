namespace Fixie.Behaviors
{
    public interface CaseBehavior
    {
        void Execute(Case @case, object instance);
    }
}