using System;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.Conventions
{
    public class CaseBehaviorBuilderTests
    {
        readonly CaseBehaviorBuilder builder;
        readonly object instance;

        public CaseBehaviorBuilderTests()
        {
            builder = new CaseBehaviorBuilder();
            instance = new SampleTestClass();
        }
        
        public void ShouldShortCircuitSetupAndInnerBehaviorAndTearDownWhenCaseAlreadyHasExceptionsPriorToSetup()
        {
            builder.SetUpTearDown(SetUp, TearDown);

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Pass");
                var exception = new Exception("Exception from earlier in the behavior chain.");
                @case.Exceptions.Add(exception);

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.ToArray().Single().ShouldEqual(exception);
                console.Lines.ShouldEqual();
            }
        }

        class SampleTestClass
        {
            public void SetUpA()
            {
                Console.WriteLine("SetUpA");
            }

            public void SetUpB()
            {
                Console.WriteLine("SetUpB");
            }

            public void Pass()
            {
                Console.WriteLine("Pass");
            }

            public void Fail()
            {
                Console.WriteLine("Fail Threw!");
                throw new FailureException();
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

        static void SetUp(Case @case, object instance)
        {
            Console.WriteLine("SetUp");
        }

        static void TearDown(Case @case, object instance)
        {
            Console.WriteLine("TearDown");
        }

        static Case Case(string methodName)
        {
            var testClass = typeof(SampleTestClass);
            return new Case(testClass, testClass.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public));
        }
    }
}