using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        public void Execute(Fixture fixture, Case[] cases, Convention convention)
        {
            foreach (var @case in cases)
                convention.CaseExecution.Behavior.Execute(@case.Method, fixture.Instance, @case.Exceptions);
        }
    }
}