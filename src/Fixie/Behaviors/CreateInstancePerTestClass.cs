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

                Lifecycle.Dispose(instance);
            }
            catch (Exception constructionException)
            {
                foreach (var @case in cases)
                    @case.Fail(constructionException);
            }
        }
    }
}