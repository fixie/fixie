namespace Fixie.Samples.Static
{
    using Conventions;

    public class CustomConvention : DefaultConvention
    {
        public CustomConvention()
        {
            Classes
                .Where(x => x.IsInNamespace(GetType().Namespace));
        }
    }
}