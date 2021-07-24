namespace Fixie
{
    using System.Threading.Tasks;

    public interface IExecution
    {
        /// <summary>
        /// Runs the given set of discovered tests.
        /// </summary>
        Task RunAsync(TestAssembly testAssembly);
    }
}