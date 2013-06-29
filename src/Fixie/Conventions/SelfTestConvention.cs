namespace Fixie.Conventions
{
    public class SelfTestConvention : Convention
    {
        public SelfTestConvention()
        {
            Classes
                .Where(fixtureClass => fixtureClass.IsNestedPrivate)
                .NameEndsWith("Fixture");

            Cases
                .Where(method => method.Void() || method.Async())
                .ZeroParameters();
        }
    }
}