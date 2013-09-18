using System.Reflection;

namespace Fixie.Behaviors
{
    public class InvokeMethod : InvokeBehavior
    {
        public object Execute(MethodInfo methodInfo, object instance)
        {
            return methodInfo.Invoke(instance, null);
        }
    }
}