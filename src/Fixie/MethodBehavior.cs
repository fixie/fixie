using System.Reflection;

namespace Fixie
{
    public interface MethodBehavior
    {
        void Execute(MethodInfo method, object instance, ExceptionList exceptions);
    }
}