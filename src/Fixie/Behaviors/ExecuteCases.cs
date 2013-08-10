using System;

namespace Fixie.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        public void Execute(Fixture fixture)
        {
            foreach (var @case in fixture.Cases)
            {
                try
                {
                    fixture.CaseExecutionBehavior.Execute(@case, fixture.Instance);
                }
                catch (PreservedException preservedException)
                {
                    @case.Fail(preservedException.OriginalException);
                }
                catch (Exception ex)
                {
                    @case.Fail(ex);
                }
            }
        }
    }
}