using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : TypeBehavior
    {
        readonly Factory construct;

        public CreateInstancePerCase(Factory construct)
        {
            this.construct = construct;
        }

        public void Execute(Type testClass, Convention convention, Case[] cases)
        {
            foreach (var @case in cases)
            {
                var exceptions = @case.Exceptions;

                object instance;

                var constructionExceptions = construct(testClass, out instance);
                if (constructionExceptions.Any())
                {
                    exceptions.Add(constructionExceptions);
                }
                else
                {
                    var fixture = new Fixture(testClass, instance, convention.CaseExecution.Behavior, new[] { @case });
                    convention.InstanceExecution.Behavior.Execute(fixture);

                    try
                    {
                        Lifecycle.Dispose(instance);
                    }
                    catch (Exception disposalException)
                    {
                        exceptions.Add(disposalException);
                    }
                }
            }
        }
    }
}