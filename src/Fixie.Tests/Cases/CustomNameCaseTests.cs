using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;

namespace Fixie.Tests.Cases
{
    public class CustomNameCaseTests : CaseTests
    {
        public void ShouldSetTheCustomName()
        {
            Convention.CaseExecution.Name(@case =>
            {
                return @case.Method.Name + ".Custom";
            });

            Run<NameTestClass>();

            Listener.Entries.ShouldEqual(
                "Fail.Custom failed: 'Fail' failed!",
                "Pass.Custom passed.");
        }

        public void ShouldFailWithClearExplanationWhenCustomNameThrows()
        {
            Convention.CaseExecution
                .Name(@case => { throw new Exception("Unsafe name generator threw!"); });

            Action attemptFaultyName = Run<NameTestClass>;

            var exception = attemptFaultyName.ShouldThrow<Exception>(
                "Exception thrown while attempting to get a custom case name. " +
                "Check the inner exception for more details.");

            exception.InnerException.Message.ShouldEqual("Unsafe name generator threw!");
        }

        public void ShouldSortByCustomName()
        {
            Convention.CaseExecution
                .Name(@case =>
                {
                    switch (@case.Method.Name)
                    {
                        case "Pass":
                            return "A";
                        case "Fail":
                            return "B";
                        default:
                            throw new Exception();
                    }
                });
            Convention.ClassExecution.SortCases((a, b) =>
            {
                return a.Name.CompareTo(b.Name);
            });

            Run<NameTestClass>();

            Listener.Entries.ElementAt(0).ShouldStartWith("A passed");
            Listener.Entries.ElementAt(1).ShouldStartWith("B failed");
        }

        class NameTestClass
        {
            public void Fail() { throw new FailureException(); }

            public void Pass() { }
        }
    }
}
