using System.Reflection;

namespace Fixie.Behaviors
{
    public interface CaseBehavior
    {
        void Execute(MethodInfo method, object instance, ExceptionList exceptions);
    }
}