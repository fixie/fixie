using System.Reflection;

namespace Fixie
{
    public class DefaultConvention : Convention
    {
        public DefaultConvention()
        {
            Fixtures
                .ConcreteClasses()
                .NameEndsWith("Tests")
                .HasDefaultConstructor();

            Cases
                .Visibility(BindingFlags.Public | BindingFlags.Instance)
                .Void()
                .ZeroParameters();
        }
    }
}