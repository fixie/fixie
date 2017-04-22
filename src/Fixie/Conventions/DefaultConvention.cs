namespace Fixie.Conventions
{
    /// <summary>
    /// The default convention is applied to a test assembly whenever the test assembly specifies no custom convention to use.
    ///
    /// <para>A class is a test class if its name ends with "Tests".</para>
    ///
    /// <para>A public instance method in a test class is a test method if it is declared void or async.</para>
    /// </summary>
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