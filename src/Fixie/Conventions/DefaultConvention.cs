namespace Fixie.Conventions
{
    /// <summary>
    /// The default convention is applied to a test assembly whenever the test assembly specifies no custom convention to use.
    ///
    /// <para>A class is a test class if its name ends with "Tests".</para>
    ///
    /// <para>All public instance methods in a test class are test methods.</para>
    /// </summary>
    public class DefaultConvention : Convention
    {
        public DefaultConvention()
            => Classes.Where(x => x.Name.EndsWith("Tests"));
    }
}