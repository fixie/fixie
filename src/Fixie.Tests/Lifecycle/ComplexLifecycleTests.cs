using System;
using Fixie.Conventions;

namespace Fixie.Tests.Lifecycle
{
    public class ComplexLifecycleTests : LifecycleTests
    {
        public void ShouldPerformCompleteLifecyclePerCaseWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap(WrapType);

            Convention.InstanceExecution
                      .Wrap(WrapInstance);

            Convention.CaseExecution
                      .Wrap(WrapCase);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Before Type",

                ".ctor",
                "Start Instance",
                "Before Case",
                "Pass",
                "After Case",
                "End Instance",
                "Dispose",

                ".ctor",
                "Start Instance",
                "Before Case",
                "Fail",
                "After Case",
                "End Instance",
                "Dispose",

                "After Type");
        }

        public void ShouldPerformCompleteLifecyclePerTestClassWhenConstructingPerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass()
                      .Wrap(WrapType);

            Convention.InstanceExecution
                      .Wrap(WrapInstance);

            Convention.CaseExecution
                      .Wrap(WrapCase);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Before Type",
                ".ctor",

                "Start Instance",
                "Before Case",
                "Pass",
                "After Case",

                "Before Case",
                "Fail",
                "After Case",
                "End Instance",

                "Dispose",
                "After Type");
        }

        static void WrapType(Type testClass, Convention convention, Case[] cases, Action innerBehavior)
        {
            Console.WriteLine("Before Type");
            innerBehavior();
            Console.WriteLine("After Type");
        }

        static void WrapInstance(Fixture fixture, Action innerBehavior)
        {
            Console.WriteLine("Start Instance");
            innerBehavior();
            Console.WriteLine("End Instance");
        }

        static void WrapCase(Case @case, object instance, Action innerBehavior)
        {
            Console.WriteLine("Before Case");
            innerBehavior();
            Console.WriteLine("After Case");
        }
    }
}