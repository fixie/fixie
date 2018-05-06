namespace Fixie.Conventions
{
    /// <summary>
    /// The default discovery rules are applied to a test assembly whenever the test assembly specifies no custom discovery rules.
    ///
    /// <para>A class is a test class if its name ends with "Tests".</para>
    ///
    /// <para>All public methods in a test class are test methods.</para>
    /// </summary>
    public class DefaultDiscovery : Discovery
    {
        public DefaultDiscovery()
            => Classes.Where(x => x.Name.EndsWith("Tests"));
    }
}