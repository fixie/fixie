using System;
using System.Diagnostics;

namespace Fixie.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        public void Execute(TestClassInstance testClassInstance)
        {
            foreach (var caseExecution in testClassInstance.CaseExecutions)
            {

                using (var console = new RedirectedConsole())
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        testClassInstance.CaseExecutionBehavior.Execute(caseExecution, testClassInstance.Instance);
                    }
                    catch (Exception exception)
                    {
                        caseExecution.Fail(exception);
                    }

                    stopwatch.Stop();

                    caseExecution.Duration = stopwatch.Elapsed;
                    caseExecution.Output = console.Output;
                }

                Console.Write(caseExecution.Output);
            }
        }
    }
}