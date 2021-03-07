namespace Fixie
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a test class lifecycle, to be executed once per test class.
    /// </summary>
    public interface Execution
    {
        Task StartAsync() => Task.CompletedTask;
        Task RunAsync(TestAssembly testAssembly);
        Task CompleteAsync() => Task.CompletedTask;
    }
}