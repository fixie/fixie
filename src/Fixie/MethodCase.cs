using System.Reflection;

namespace Fixie
{
    public class MethodCase : Case
    {
        readonly string fixtureName;
        readonly MethodInfo method;

        public MethodCase(Fixture fixture, MethodInfo method)
        {
            fixtureName = fixture.Name;
            this.method = method;
        }

        public string Name
        {
            get { return fixtureName + "." + method.Name; }
        }
    }
}