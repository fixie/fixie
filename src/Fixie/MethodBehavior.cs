using System.Reflection;

namespace Fixie
{
    public interface MethodBehavior
    {
        void Execute(MethodInfo method, object fixtureInstance, ExceptionList exceptions);
    }
}