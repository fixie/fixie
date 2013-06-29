namespace Fixie.Conventions
{
    public class SelfTestConvention : Convention
    {
        public SelfTestConvention()
        {
            Classes
                .Where(testClass => testClass.IsNestedPrivate)
                .NameEndsWith("TestClass");

            Cases
                .Where(method => method.Void() || method.Async())
                .ZeroParameters();
        }
    }
}