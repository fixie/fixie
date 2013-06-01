using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        public void Execute(Type fixtureClass, object instance, Case[] cases, Convention convention)
        {
            foreach (var @case in cases)
                convention.CaseExecution.Behavior.Execute(@case.Method, instance, @case.Exceptions);
        }
    }
}