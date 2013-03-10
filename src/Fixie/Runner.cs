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

        public Result Execute(Convention convention)
        {
            var passed = 0;
            var failed = 0;

            foreach (var fixture in convention.Fixtures)
            {
                foreach (var @case in fixture.Cases)
                {
                    if (Execute(@case))
                        passed++;
                    else
                        failed++;
                }
            }

            return new Result(passed, failed);
        }

        private bool Execute(Case @case)
        {
            try
            {
                var result = @case.Execute();

                if (result.Passed)
                    return true;

                listener.CaseFailed(@case, result.Exception);
            }
            catch (Exception ex)
            {
                listener.CaseFailed(@case, ex);
            }

            return false;
        }
    }
}