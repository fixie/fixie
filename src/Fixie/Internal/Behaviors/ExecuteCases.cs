using System;
using System.Diagnostics;

namespace Fixie.Internal.Behaviors
{
    public class ExecuteCases : FixtureBehavior
    {
        readonly BehaviorChain<Case> caseBehaviors;

        public ExecuteCases(BehaviorChain<Case> caseBehaviors)
        {
            this.caseBehaviors = caseBehaviors;
        }

        public void Execute(Fixture fixture, Action next)
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
                        caseBehaviors.Execute(@case);
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