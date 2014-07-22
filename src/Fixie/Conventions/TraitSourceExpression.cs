namespace Fixie.Conventions
{
    public class TraitSourceExpression
    {
        readonly Configuration config;

        public TraitSourceExpression(Configuration config)
        {
            this.config = config;
        }

        public TraitSourceExpression Add<TTraitSource>() where TTraitSource : TraitSource
        {
            config.AddTraitSource<TTraitSource>();
            return this;
        }
    }
}