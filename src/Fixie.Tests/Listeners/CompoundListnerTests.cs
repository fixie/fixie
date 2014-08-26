using Fixie.Execution;
using Fixie.Listeners;
using Fixie.Results;
using Should;
using System.Linq;
using System.Reflection;

namespace Fixie.Tests.Listeners
{
    public class CompoundListnerTests
    {
        public void ShouldCallAssemblyStartedOnUnderlyingListerners()
        {
            var listeners = new[] { new TestListener(), new TestListener() };
            var compoundListener = new CompoundListener(listeners);
            compoundListener.AssemblyStarted(null);
            listeners.All(x => x.AssemblyStartedCalled).ShouldBeTrue();
        }

        public void ShouldCallCaseSkippedOnUnderlyingListerners()
        {
            var listeners = new[] { new TestListener(), new TestListener() };
            var compoundListener = new CompoundListener(listeners);
            compoundListener.CaseSkipped(null);
            listeners.All(x => x.CaseSkippedCalled).ShouldBeTrue();
        }

        public void ShouldCallCasePassedOnUnderlyingListerners()
        {
            var listeners = new[] { new TestListener(), new TestListener() };
            var compoundListener = new CompoundListener(listeners);
            compoundListener.CasePassed(null);
            listeners.All(x => x.CasePassedCalled).ShouldBeTrue();
        }

        public void ShouldCallCaseFailedOnUnderlyingListerners()
        {
            var listeners = new[] { new TestListener(), new TestListener() };
            var compoundListener = new CompoundListener(listeners);
            compoundListener.CaseFailed(null);
            listeners.All(x => x.CaseFailedCalled).ShouldBeTrue();
        }

        public void ShouldCallAssemblyCompletedOnUnderlyingListerners()
        {
            var listeners = new[] { new TestListener(), new TestListener() };
            var compoundListener = new CompoundListener(listeners);
            compoundListener.AssemblyCompleted(null, null);
            listeners.All(x => x.AssemblyCompletedCalled).ShouldBeTrue();
        }

        class TestListener : Listener
        {
            public bool AssemblyStartedCalled { get; private set; }
            public bool CaseSkippedCalled { get; private set; }
            public bool CasePassedCalled { get; private set; }
            public bool CaseFailedCalled { get; private set; }
            public bool AssemblyCompletedCalled { get; private set; }

            public void AssemblyStarted(Assembly assembly)
            {
                AssemblyStartedCalled = true;
            }

            public void CaseSkipped(SkipResult result)
            {
                CaseSkippedCalled = true;
            }

            public void CasePassed(PassResult result)
            {
                CasePassedCalled = true;
            }

            public void CaseFailed(FailResult result)
            {
                CaseFailedCalled = true;
            }

            public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
            {
                AssemblyCompletedCalled = true;
            }
        }
    }
}