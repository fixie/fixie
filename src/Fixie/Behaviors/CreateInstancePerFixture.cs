using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerFixture : TypeBehavior
    {
        readonly Factory construct;

        public CreateInstancePerFixture(Factory construct)
        {
            this.construct = construct;
        }

        public void Execute(Type fixtureClass, Convention convention, Case[] cases)
        {
            object instance;

            var constructionExceptions = construct(fixtureClass, out instance);
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