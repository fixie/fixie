namespace Fixie.Conventions
{
    public class DefaultConvention : Convention
    {
        public DefaultConvention()
        {
            Classes
                .NameEndsWith("Tests");

            Cases = new MethodFilter()
                .Where(method => method.Void() || method.Async())
                .ZeroParameters();
        }
    }
}