using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Fixie.Behaviors;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.Behaviors
{
    public class MethodFilterExtensionsTests
    {
        public void ShouldInvokeAllMethodsIdentifiedByTheFilter()
        {
            var passingMethods = new MethodFilter().Where(m => m.Name.StartsWith("Pass"));

            using (var console = new RedirectedConsole())
            {
                var exceptions = passingMethods.InvokeAll(typeof(SampleFixture), new SampleFixture());

                exceptions.ToArray().ShouldBeEmpty();

                console.Lines.ShouldEqual("PassA", "PassB", "PassC");
            }
        }

        public void ShouldCollectExceptionsFromAllInvokedMethods()
        {
            var passingMethods = new MethodFilter().Where(m => m.Name.StartsWith("Fail"));

            using (var console = new RedirectedConsole())
            {
                var exceptions = passingMethods.InvokeAll(typeof(SampleFixture), new SampleFixture());

                exceptions.ToArray().Select(x => x.Message).ShouldEqual("'FailA' failed!", "'FailB' failed!", "'FailC' failed!");

                console.Lines.ShouldEqual("FailA", "FailB", "FailC");
            }
        }

        class SampleFixture
        {
            public void PassA() { WhereAmI(); }
            public void PassB() { WhereAmI(); }
            public void PassC() { WhereAmI(); }

            public void FailA() { WhereAmI(); throw new FailureException(); }
            public void FailB() { WhereAmI(); throw new FailureException(); }
            public void FailC() { WhereAmI(); throw new FailureException(); }

            static void WhereAmI([CallerMemberName] string method = null)
            {
                Console.WriteLine(method);
            }
        }
    }
}