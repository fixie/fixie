using System;
using System.Collections.Generic;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : TypeBehavior
    {
        readonly Func<Type, object> construct;

        public CreateInstancePerCase(Func<Type, object> construct)
        {
            this.construct = construct;
        }

        public void Execute(Type testClass, Convention convention, Case[] cases)
        {
            foreach (var @case in cases)
            {
                try
                {
                    var instance = construct(testClass);

                    var fixture = new Fixture(testClass, instance, convention.CaseExecution.Behavior, new[] { @case });
                    convention.InstanceExecution.Behavior.Execute(fixture);

                    Lifecycle.Dispose(instance);
                }
                catch (PreservedException preservedException)
                {
                    var constructionException = preservedException.OriginalException;
                    @case.Fail(constructionException);
                }
                catch (Exception constructionException)
                {
                    @case.Fail(constructionException);
                }
            }
        }
    }
}