using System.Threading.Tasks;

namespace Fixie;

public interface IExecution
{
    /// <summary>
    /// Runs the given set of discovered tests.
    /// </summary>
    Task Run(TestSuite testSuite);
}