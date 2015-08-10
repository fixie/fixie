namespace Fixie.Samples
{
    public class SamplesAssembly : TestAssembly
    {
        public SamplesAssembly()
        {
            Apply<Categories.CustomConvention>();
            Apply<ColumnParameter.CustomConvention>();
            Apply<Inclusive.CustomConvention>();
            Apply<IoC.CustomConvention>();
            Apply<LowCeremony.CustomConvention>();
            Apply<MbUnitStyle.CustomConvention>();
            Apply<NUnitStyle.CustomConvention>();
            Apply<Parameterized.CustomConvention>();
            Apply<Shuffle.CustomConvention>();
            Apply<Skipped.CustomConvention>();
            Apply<xUnitStyle.CustomConvention>();
        }
    }
}
