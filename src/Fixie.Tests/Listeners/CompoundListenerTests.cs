using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Fixie.Listeners;
using Fixie.Results;

namespace Fixie.Tests.Listeners
{
    public class CompoundListenerTests
    {
        readonly Assembly assembly;
        readonly Case @case;
        readonly PassResult pass;
        readonly FailResult fail;

        public CompoundListenerTests()
        {
            var testClass = typeof(CompoundListenerTests);
            assembly = testClass.Assembly;
            @case = new Case(testClass, testClass.GetInstanceMethod("ShouldRepeatEachEventToEachInnerListener"));
            pass = new PassResult(new CaseExecution(@case));
            var failedExecution = new CaseExecution(@case);
            failedExecution.Fail(new Exception("Sample failure."));
            fail = new FailResult(failedExecution);
        }

        public void ShouldRepeatEachEventToEachInnerListener()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new CompoundListener();
                listener.Add(new SampleListenerA());
                listener.Add(new SampleListenerB());

                var assemblyResult = new AssemblyResult(assembly.Location);
                var conventionResult = new ConventionResult("Fake Convention");
                var classResult = new ClassResult("Fake Class");
                assemblyResult.Add(conventionResult);
                conventionResult.Add(classResult);
                classResult.Add(CaseResult.Passed("A", TimeSpan.Zero));
                classResult.Add(CaseResult.Passed("B", TimeSpan.Zero));
                classResult.Add(CaseResult.Passed("C", TimeSpan.Zero));
                classResult.Add(CaseResult.Failed("D", TimeSpan.Zero, "Message", "Stack Trace"));
                classResult.Add(CaseResult.Failed("E", TimeSpan.Zero, "Message", "Stack Trace"));
                classResult.Add(CaseResult.Skipped("F"));


                listener.AssemblyStarted(assembly);
                listener.CaseSkipped(@case);
                listener.CasePassed(pass);
                listener.CaseFailed(fail);
                listener.AssemblyCompleted(assembly, assemblyResult);

                console.Lines()
                    .ShouldEqual(
                        "SampleListenerA.AssemblyStarted: Fixie.Tests",
                        "SampleListenerB.AssemblyStarted: Fixie.Tests",
                        "SampleListenerA.CaseSkipped: Fixie.Tests.Listeners.CompoundListenerTests.ShouldRepeatEachEventToEachInnerListener",
                        "SampleListenerB.CaseSkipped: Fixie.Tests.Listeners.CompoundListenerTests.ShouldRepeatEachEventToEachInnerListener",
                        "SampleListenerA.CasePassed: Fixie.Tests.Listeners.CompoundListenerTests.ShouldRepeatEachEventToEachInnerListener",
                        "SampleListenerB.CasePassed: Fixie.Tests.Listeners.CompoundListenerTests.ShouldRepeatEachEventToEachInnerListener",
                        "SampleListenerA.CaseFailed: Fixie.Tests.Listeners.CompoundListenerTests.ShouldRepeatEachEventToEachInnerListener: Sample failure.",
                        "SampleListenerB.CaseFailed: Fixie.Tests.Listeners.CompoundListenerTests.ShouldRepeatEachEventToEachInnerListener: Sample failure.",
                        "SampleListenerA.AssemblyCompleted: Fixie.Tests 3 passed, 2 failed, 1 skipped",
                        "SampleListenerB.AssemblyCompleted: Fixie.Tests 3 passed, 2 failed, 1 skipped");
            }
        }

        abstract class SampleListener : Listener
        {
            public void AssemblyStarted(Assembly assembly)
            {
                WhereAmI(assembly.GetName().Name);
            }

            public void CaseSkipped(Case @case)
            {
                WhereAmI(@case.Name);
            }

            public void CasePassed(PassResult result)
            {
                WhereAmI(result.Case.Name);
            }

            public void CaseFailed(FailResult result)
            {
                WhereAmI(result.Case.Name + ": " + result.Exceptions.Single().Message);
            }

            public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
            {
                WhereAmI(string.Format("{0} {1} passed, {2} failed, {3} skipped", assembly.GetName().Name, result.Passed, result.Failed, result.Skipped));
            }

            void WhereAmI(string detail, [CallerMemberName] string member = null)
            {
                Console.WriteLine(GetType().Name + "." + member + ": " + detail);
            }
        }

        class SampleListenerA : SampleListener { }
        class SampleListenerB : SampleListener { }
    }
}