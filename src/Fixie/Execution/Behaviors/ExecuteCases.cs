namespace Fixie.Execution.Behaviors
{
    using System;
    using System.Diagnostics;

    class ExecuteCases
    {
        readonly BehaviorChain<Case> caseBehaviors;

        public ExecuteCases(BehaviorChain<Case> caseBehaviors = null)
        {
            this.caseBehaviors = caseBehaviors;
        }

        public void Execute(Fixture fixture, Action next)
        {
            Execute(fixture, caseBehaviors.Execute);
        }

        public void Execute(Fixture fixture, CaseAction caseLifecycle)
        {
            foreach (var @case in fixture.Cases)
            {
                using (var console = new RedirectedConsole())
                {
                    @case.Fixture = fixture;

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        caseLifecycle(@case);
                    }
                    catch (Exception exception)
                    {
                        @case.Fail(exception);
                    }

                    stopwatch.Stop();

                    @case.Fixture = null;
                    @case.Duration += stopwatch.Elapsed;
                    @case.Output = console.Output;
                }

                Console.Write(@case.Output);
            }
        }
    }
}