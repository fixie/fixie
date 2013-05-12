using System;
using System.Reflection;

namespace Fixie
{
    public class Invoke : FixtureCommand
    {
        readonly MethodInfo method;

        public Invoke(MethodInfo method)
        {
            this.method = method;
        }

        public void Execute(object fixtureInstance, ExceptionList exceptions)
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