using System.Reflection;

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
                    catch (TargetInvocationException ex)
                    {
                        listener.CaseFailed(@case, ex.InnerException);
                        failed++;
                    }
                }
            }

            return new Result(passed, failed);
        }
    }
}