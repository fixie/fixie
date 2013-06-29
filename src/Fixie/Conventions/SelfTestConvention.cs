namespace Fixie.Conventions
{
    public class SelfTestConvention : Convention
    {
        public SelfTestConvention()
        {
            Classes
                .Where(testClass => testClass.IsNestedPrivate)
                .NameEndsWith("Fixture");

            Cases
                .Where(method => method.Void() || method.Async())
                .ZeroParameters();
        }
    }
}