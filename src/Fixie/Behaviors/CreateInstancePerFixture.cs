using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerFixture : TypeBehavior
    {
        public void Execute(Type fixtureClass, Convention convention, Case[] cases)
        {
            object instance;

            var constructionExceptions = Lifecycle.Construct(fixtureClass, out instance);
            if (constructionExceptions.Any())
            {
                foreach (var @case in cases)
                    @case.Exceptions.Add(constructionExceptions);
            }
            else
            {
                convention.InstanceExecution.Behavior.Execute(fixtureClass, instance, cases, convention);

                var disposalExceptions = Lifecycle.Dispose(instance);
                if (disposalExceptions.Any())
                {
                    foreach (var @case in cases)
                        @case.Exceptions.Add(disposalExceptions);
                }
            }
        }
    }
}