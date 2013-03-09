using System;

namespace Fixie
{
    public class Runner
    {
        readonly Listener listener;

        public Runner(Listener listener)
        {
            this.listener = listener;
        }

        public Result Execute(Configuration configuration)
        {
            var passed = 0;
            var failed = 0;

            foreach (var fixture in configuration.Fixtures)
            {
                foreach (var @case in fixture.Cases)
                {
                    try
                    {
                        @case.Execute();
                        passed++;
                    }
                    catch (Exception ex)
                    {
                        listener.CaseFailed(@case, ex);
                        failed++;
                    }
                }
            }

            return new Result(passed, failed);
        }
    }
}