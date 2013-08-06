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
                    @case.Exceptions.Add(preservedException.OriginalException);
                }
                catch (Exception ex)
                {
                    @case.Exceptions.Add(ex);
                }
            }
        }
    }
}