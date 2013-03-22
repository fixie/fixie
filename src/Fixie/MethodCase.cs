using System;
using System.Reflection;

namespace Fixie
{
    public class MethodCase : Case
    {
        private readonly ClassFixture fixture;
        private readonly MethodInfo method;

        public MethodCase(ClassFixture fixture, MethodInfo method)
        {
            this.fixture = fixture;
            this.method = method;
        }

        public string Name
        {
            get { return fixture.Name + "." + method.Name; }
        }

        public Result Execute(Listener listener)
        {
            try
            {
                method.Invoke(fixture.Instance, null);
                return Result.Pass;
            }
            catch (TargetInvocationException ex)
            {
                listener.CaseFailed(this, ex.InnerException);
                return Result.Fail;
            }
            catch (Exception ex)
            {
                listener.CaseFailed(this, ex);
                return Result.Fail;
            }
        }
    }
}