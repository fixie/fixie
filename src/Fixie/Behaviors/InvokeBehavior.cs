using System.Reflection;

namespace Fixie.Behaviors
{
    public interface InvokeBehavior
    {
        object Execute(MethodInfo methodInfo, object instance);
    }
}