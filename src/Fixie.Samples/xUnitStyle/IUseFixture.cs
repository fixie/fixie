namespace Fixie.Samples.xUnitStyle
{
    public interface IUseFixture<T> where T : new()
    {
        void SetFixture(T data);
    }
}