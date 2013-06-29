namespace Fixie.Conventions
{
    public class DefaultConvention : Convention
    {
        public DefaultConvention()
        {
            Classes
                .NameEndsWith("Tests");

            Cases
                .Where(method => method.Void() || method.Async())
                .ZeroParameters();
        }
    }
}