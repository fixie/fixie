namespace Fixie.Behaviors
{
    public interface CaseBehavior
    {
        void Execute(InvokeBehavior invokeBehavior, Case @case, object instance);
    }
}