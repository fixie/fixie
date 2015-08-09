using System;
using System.Reflection;
using Should;

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
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.ExplicitAndSkip skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Explicit passed.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed.");
        }

        public void ShouldSkipCasesWithOptionalReason()
        {
            Convention.CaseExecution
                .Skip(HasSkipAttribute, SkipAttributeReason);

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.ExplicitAndSkip skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail skipped: Troublesome test skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Explicit passed.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed.");
        }

        public void ShouldAllowMultiplePrioritizedSkipRules()
        {
            Convention.CaseExecution
                .Skip(HasExplicitAttribute, ExplicitAttributeReason)
                .Skip(HasSkipAttribute, SkipAttributeReason);

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Explicit skipped: [Explicit] tests run only when they are individually selected for execution.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.ExplicitAndSkip skipped: [Explicit] tests run only when they are individually selected for execution.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail skipped: Troublesome test skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed.");
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

        static string ExplicitAttributeReason(Case @case)
        {
            return "[Explicit] tests run only when they are individually selected for execution.";
        }

        static bool HasExplicitAttribute(Case @case)
        {
            return @case.Method.HasOrInherits<ExplicitAttribute>();
        }

        static string SkipAttributeReason(Case @case)
        {
            var method = @case.Method;

            var target = method.HasOrInherits<SkipAttribute>() ? (MemberInfo)method : method.DeclaringType;

            return target.GetCustomAttribute<SkipAttribute>(true).Reason;
        }

        static bool HasSkipAttribute(Case @case)
        {
            return @case.Method.HasOrInherits<SkipAttribute>();
        }

        class SkippedTestClass
        {
            [Skip(Reason = "Troublesome test skipped.")]
            public void Fail() { throw new FailureException(); }

            public void Pass() { }

            [Explicit]
            public void Explicit() { }

            [Explicit]
            [Skip]
            public void ExplicitAndSkip() { }
        }

        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
        class ExplicitAttribute : Attribute
        {
            public string Reason { get; set; }
        }

        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
        class SkipAttribute : Attribute
        {
            public string Reason { get; set; }
        }
    }
}