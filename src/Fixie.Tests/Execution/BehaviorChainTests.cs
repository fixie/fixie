namespace Fixie.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Assertions;
    using Fixie.Execution;
    using Internal;

    public class BehaviorChainTests
    {
        public void ShouldDoNothingWhenEmpty()
        {
            var context = new BreadCrumbs();
            var chain = new BehaviorChain<BreadCrumbs>();

            chain.Execute(context);

            context.Crumbs.ShouldBeEmpty();
            context.Exceptions.ShouldBeEmpty();
        }

        public void ShouldExecuteBehaviorsInOrder()
        {
            var context = new BreadCrumbs();
            var chain = new BehaviorChain<BreadCrumbs>(
                new OuterBehavior(),
                new InnerBehavior()
            );

            chain.Execute(context);

            context.Crumbs.ShouldEqual(
                "Entering OuterBehavior",
                "Entering InnerBehavior",
                "Leaving InnerBehavior",
                "Leaving OuterBehavior");
        }

        public void ShouldHandleUncaughtExceptionsAtEachStepInTheChainByLoggingThemToTheContextObject()
        {
            var context = new BreadCrumbs();
            var chain = new BehaviorChain<BreadCrumbs>(
                new OuterBehavior(),
                new InnerBehavior(),
                new BuggyBehavior()
            );

            chain.Execute(context);

            context.Crumbs.ShouldEqual(
                "Entering OuterBehavior",
                "Entering InnerBehavior",
                "Entering BuggyBehavior",
                "Leaving InnerBehavior",
                "Leaving OuterBehavior");

            context.Exceptions.Single().Message.ShouldEqual("BuggyBehavior Threw!");
        }

        class BreadCrumbs : BehaviorContext
        {
            readonly List<string> crumbs = new List<string>();
            readonly List<Exception> exceptions = new List<Exception>();

            public IEnumerable<string> Crumbs { get { return crumbs; } } 
            public IEnumerable<Exception> Exceptions { get { return exceptions; } }

            public void Add(string crumb)
            {
                crumbs.Add(crumb);
            }

            public void Fail(Exception reason)
            {
                exceptions.Add(reason);
            }
        }

        class InnerBehavior : Behavior<BreadCrumbs>
        {
            public void Execute(BreadCrumbs context, Action next)
            {
                context.Add("Entering " + GetType().Name);
                next();
                context.Add("Leaving " + GetType().Name);
            }
        }

        class OuterBehavior : Behavior<BreadCrumbs>
        {
            public void Execute(BreadCrumbs context, Action next)
            {
                context.Add("Entering " + GetType().Name);
                next();
                context.Add("Leaving " + GetType().Name);
            }
        }

        class BuggyBehavior : Behavior<BreadCrumbs>
        {
            public void Execute(BreadCrumbs context, Action next)
            {
                context.Add("Entering " + GetType().Name);
                throw new Exception(GetType().Name + " Threw!");
            }
        }
    }
}