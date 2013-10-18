using System;
using System.Diagnostics;

namespace Fixie.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        public void Execute(Fixture fixture)
        {
            foreach (var @case in fixture.Cases)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    fixture.CaseExecutionBehavior.Execute(@case, fixture.Instance);
                }
                catch (Exception exception)
                {
                    @case.Fail(exception);
                }

                stopwatch.Stop();
                @case.Duration = stopwatch.Elapsed;
            }
        }
    }
}