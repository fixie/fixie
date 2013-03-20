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
            var result = new Result(0, 0);

            foreach (var fixture in convention.Fixtures)
                foreach (var @case in fixture.Cases)
                    result = Result.Combine(result, Execute(@case));

            return result;
        }

        private Result Execute(Case @case)
        {
            try
            {
                return @case.Execute(listener);
            }
            catch (Exception ex)
            {
                listener.CaseFailed(@case, ex);
                return Result.Fail;
            }
        }
    }
}