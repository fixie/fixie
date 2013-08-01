using System;
using System.Linq;
using System.Reflection;
using Fixie.Behaviors;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.Conventions
{
    public class InstanceBehaviorBuilderTests
    {
        readonly InstanceBehaviorBuilder builder;
        readonly Fixture fixture;

        public InstanceBehaviorBuilderTests()
        {
            builder = new InstanceBehaviorBuilder();
            var testClass = typeof(SampleTestClass);
            var instance = new SampleTestClass();
            var caseExecutionBehavior = new Invoke();
            fixture = new Fixture(testClass, instance, caseExecutionBehavior,  new[]
            {
                new Case(testClass, Method("Pass")),
                new Case(testClass, Method("Fail"))
            });
        }

        public void ShouldAllowSetUpTearDownByInvokingAllMethodsFoundByMethodFilters()
        {
            var setUp = new MethodFilter().Where(m => m.Name.StartsWith("SetUp"));
            var tearDown = new MethodFilter().Where(m => m.Name.StartsWith("TearDown"));

            builder.SetUpTearDown(setUp, tearDown);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixture);

                fixture.Cases[0].Exceptions.Any().ShouldBeFalse();
                fixture.Cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("SetUpA", "SetUpB", "Pass", "Fail", "TearDownA", "TearDownB");
            }
        }

        class SampleTestClass
        {
            public void Pass()
            {
                Console.WriteLine("Pass");
            }

            public void Fail()
            {
                Console.WriteLine("Fail");
                throw new FailureException();
            }

            public void SetUpA()
            {
                Console.WriteLine("SetUpA");
            }

            public void SetUpB()
            {
                Console.WriteLine("SetUpB");
            }

            public void TearDownA()
            {
                Console.WriteLine("TearDownA");
            }

            public void TearDownB()
            {
                Console.WriteLine("TearDownB");
            }
        }

        static MethodInfo Method(string name)
        {
            return typeof(SampleTestClass).GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
        }
    }
}