using System;
using System.Reflection;
using Should;
using System.Linq;

namespace Fixie.Tests.Cases
{
    public class SkippedCaseTests : CaseTests
    {
        public void ShouldSkipCases()
        {
            Convention.CaseExecution
                .Skip(HasSkipAttribute);

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed.");
        }

        public void ShouldSkipCasesWithOptionalReason()
        {
            Convention.CaseExecution
                .Skip(HasSkipAttribute, SkipAttributeReason);

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail skipped: Troublesome test skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed.");
        }

        public void ShouldSkipCasesAfterRunning()
        {
            Convention.CaseExecution
                .Skip(ThrewSkipException);

            Run<SkippedWithExceptionTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedWithExceptionTestClass.Skip skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedWithExceptionTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedWithExceptionTestClass.Pass passed.");
        }

        public void ShouldSkipCasesAfterRunningWithOptionalReason()
        {
            Convention.CaseExecution
                .Skip(ThrewSkipException, SkipExceptionReason);

            Run<SkippedWithExceptionTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedWithExceptionTestClass.Skip skipped: Troublesome test skipped by throwing exception",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedWithExceptionTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedWithExceptionTestClass.Pass passed.");
        }

        public void ShouldFailWithClearExplanationWhenSkipConditionThrows()
        {
            Convention.CaseExecution
                .Skip(@case => { throw new Exception("Unsafe case-skipping predicate threw!"); });

            Action attemptFaultySkip = Run<SkippedTestClass>;

            var exception = attemptFaultySkip.ShouldThrow<Exception>(
                "Exception thrown while attempting to run a custom case-skipping predicate. " +
                "Check the inner exception for more details.");

            exception.InnerException.Message.ShouldEqual("Unsafe case-skipping predicate threw!");
        }

        public void ShouldFailWithClearExplanationWhenSkipReasonThrows()
        {
            Convention.CaseExecution
                .Skip(HasSkipAttribute, @case => { throw new Exception("Unsafe case-skipped reason generator threw!"); });

            Action attemptFaultySkip = Run<SkippedTestClass>;

            var exception = attemptFaultySkip.ShouldThrow<Exception>(
                "Exception thrown while attempting to get a custom case-skipped reason. " +
                "Check the inner exception for more details.");

            exception.InnerException.Message.ShouldEqual("Unsafe case-skipped reason generator threw!");
        }

        static string SkipAttributeReason(Case @case)
        {
            var method = @case.Method;

            var target = method.HasOrInherits<SkipAttribute>() ? (MemberInfo)method : method.DeclaringType;

            return target.GetCustomAttribute<SkipAttribute>(true).Reason;
        }

        static bool HasSkipAttribute(Case @case)
        {
            return @case.Method.HasOrInherits<SkipAttribute>() || @case.Method.DeclaringType.HasOrInherits<SkipAttribute>();
        }

        class SkippedTestClass
        {
            [Skip(Reason = "Troublesome test skipped.")]
            public void Fail() { throw new FailureException(); }

            public void Pass() { }
        }

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        class SkipAttribute : Attribute
        {
            public string Reason { get; set; }
        }

        class SkippedWithExceptionTestClass
        {
            public void Skip() { throw new SkipException("Troublesome test skipped by throwing exception"); }

            public void Fail() { throw new FailureException(); }

            public void Pass() { }
        }

        class SkipException : Exception
        {
            public SkipException(string message) : base(message) { }
        }

        static bool ThrewSkipException(Case @case)
        {
            return @case.Exceptions.Any(e => e is SkipException);
        }

        static string SkipExceptionReason(Case @case)
        {
            return @case.Exceptions.OfType<SkipException>().First().Message;
        }
    }
}