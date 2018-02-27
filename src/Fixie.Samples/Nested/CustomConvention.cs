namespace Fixie.Samples.Nested
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .Where(x => x.IsInNamespace(GetType().Namespace))
                .Where(x => x.Name.EndsWith("Tests"));
        }
    }
}