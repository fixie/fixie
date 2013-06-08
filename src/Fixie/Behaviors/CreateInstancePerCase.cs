using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : TypeBehavior
    {
        public void Execute(Type fixtureClass, Convention convention, Case[] cases)
        {
            foreach (var @case in cases)
            {
                var exceptions = @case.Exceptions;

                object instance;

                var constructionExceptions = Lifecycle.Construct(fixtureClass, out instance);
                if (constructionExceptions.Any())
                {
                    exceptions.Add(constructionExceptions);
                }
                else
                {
                    convention.InstanceExecution.Behavior.Execute(fixtureClass, instance, new[] { @case }, convention);

                    var disposalExceptions = Lifecycle.Dispose(instance);
                    if (disposalExceptions.Any())
                        exceptions.Add(disposalExceptions);
                }
            }
        }
    }
}