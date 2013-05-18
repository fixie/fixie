using System.Reflection;

namespace Fixie.Behaviors
{
    public interface MethodBehavior
    {
        void Execute(MethodInfo method, object instance, ExceptionList exceptions);
    }
}