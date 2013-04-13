using System;
using System.Reflection;

namespace Fixie
{
    public class MethodCase : Case
    {
        readonly ClassFixture fixture;
        readonly MethodInfo method;

        public MethodCase(ClassFixture fixture, MethodInfo method)
        {
            this.fixture = fixture;
            this.method = method;
        }

        public string Name
        {
            get { return fixture.Name + "." + method.Name; }
        }

        public void Execute(Listener listener)
        {
            try
            {
                method.Invoke(fixture.Instance, null);
                listener.CasePassed(this);
            }
            catch (TargetInvocationException ex)
            {
                listener.CaseFailed(this, ex.InnerException);
            }
            catch (Exception ex)
            {
                listener.CaseFailed(this, ex);
            }
        }
    }
}