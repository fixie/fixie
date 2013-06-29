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

        public void Execute(Type testClass, Convention convention, Case[] cases)
        {
            object instance;

            var constructionExceptions = construct(testClass, out instance);
            if (constructionExceptions.Any())
            {
                foreach (var @case in cases)
                    @case.Exceptions.Add(constructionExceptions);
            }
            else
            {
                var fixture = new Fixture(testClass, instance, convention.CaseExecution.Behavior, cases);
                convention.InstanceExecution.Behavior.Execute(fixture);

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