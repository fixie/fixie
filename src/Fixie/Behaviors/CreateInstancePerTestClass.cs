using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerTestClass : TypeBehavior
    {
        readonly Func<Type, object> construct;

        public CreateInstancePerTestClass(Func<Type, object> construct)
        {
            this.construct = construct;
        }

        public void Execute(Type testClass, Convention convention, Case[] cases)
        {
            try
            {
                var instance = construct(testClass);

                var fixture = new Fixture(testClass, instance, convention.CaseExecution.Behavior, cases);
                convention.InstanceExecution.Behavior.Execute(fixture);

                //TODO: Further opportunity for simplification: consider not bothering with this try/catch.
                try
                {
                    Lifecycle.Dispose(instance);
                }
                catch (Exception disposalException)
                {
                    foreach (var @case in cases)
                        @case.Exceptions.Add(disposalException);
                }
            }
            catch (PreservedException preservedException)
            {
                var constructionException = preservedException.OriginalException;

                foreach (var @case in cases)
                    @case.Exceptions.Add(constructionException);
            }
            catch (Exception constructionException)
            {
                foreach (var @case in cases)
                    @case.Exceptions.Add(constructionException);
            }
        }
    }
}