using System;
using System.Reflection;

namespace Fixie
{
    public class Invoke : MethodBehavior
    {
        public void Execute(MethodInfo method, object fixtureInstance, ExceptionList exceptions)
        {
            try
            {
                method.Invoke(fixtureInstance, null);
            }
            catch (TargetInvocationException ex)
            {
                exceptions.Add(ex.InnerException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
    }
}