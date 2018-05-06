namespace Fixie
{
    /// <summary>
    /// Defines a test class lifecycle, to be executed once per test class.
    /// </summary>
    public interface Lifecycle
    {
        void Execute(TestClass testClass);
    }
}