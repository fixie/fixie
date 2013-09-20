namespace Fixie.Conventions
{
    public class DefaultConvention : Convention
    {
        public DefaultConvention()
        {
            Classes
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid() || method.IsAsync());
        }
    }
}