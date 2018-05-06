namespace Fixie
{
    /// <summary>
    /// Defines a test class lifecycle, to be executed once per test class.
    /// </summary>
    public interface Execution
    {
        void Execute(TestClass testClass);
    }
}