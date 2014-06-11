namespace Fixie.Conventions
{
    public class ConfigModel
    {
        public ConfigModel()
        {
            ConstructionFrequency = ConstructionFrequency.PerCase;
        }

        public ConstructionFrequency ConstructionFrequency { get; set; }
    }
}