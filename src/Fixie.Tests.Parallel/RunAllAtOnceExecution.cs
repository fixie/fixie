namespace Fixie.Tests.Parallel
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class RunAllAtOnceExecution : IExecution
    {
        public async Task Run(TestSuite testSuite)
        {
            var runningTests = new List<Task>();

            foreach (var test in testSuite.Tests)
            {
                runningTests.Add(test.Run());
            }

            while (runningTests.Count > 0)
            {
                runningTests.Remove(await Task.WhenAny(runningTests));
            }
        }
    }
}
