namespace Fixie.Conventions
{
    public class DefaultConvention : Convention
    {
        public DefaultConvention()
        {
            Fixtures
                .NameEndsWith("Tests")
                .HasDefaultConstructor();

            Cases
                .Where(method => method.Void() || method.Async())
                .ZeroParameters();
        }
    }
}