namespace Fixie.Tests.Cases
{
    using System;
    using System.Reflection;

    public class SkippedCaseTests : CaseTests
    {
        public void ShouldSkipCases()
        {
            Convention.CaseExecution
                .Skip(HasSkipAttribute);

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                For<SkippedTestClass>(
                    ".Explicit passed",
                    ".ExplicitAndSkip skipped",
                    ".Fail skipped",
                    ".Pass passed"));
        }

        public void ShouldSkipCasesWithOptionalReason()
        {
            Convention.CaseExecution
                .Skip(HasSkipAttribute, SkipAttributeReason);

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                For<SkippedTestClass>(
                    ".Explicit passed",
                    ".ExplicitAndSkip skipped",
                    ".Fail skipped: Troublesome test skipped.",
                    ".Pass passed"));
        }

        public void ShouldAllowMultiplePrioritizedSkipBehaviors()
        {
            Convention.CaseExecution
                .Skip(HasExplicitAttribute, ExplicitAttributeReason)
                .Skip(HasSkipAttribute, SkipAttributeReason);

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                For<SkippedTestClass>(
                    ".Explicit skipped: [Explicit] tests run only when they are individually selected for execution.",
                    ".ExplicitAndSkip skipped: [Explicit] tests run only when they are individually selected for execution.",
                    ".Fail skipped: Troublesome test skipped.",
                    ".Pass passed"));
        }

        public void ShouldFailCaseWhenSkipConditionThrows()
        {
            Convention.CaseExecution
                .Skip(@case => throw new Exception("Unsafe case-skipping predicate threw!"));

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                For<SkippedTestClass>(
                    ".Explicit failed: Unsafe case-skipping predicate threw!",
                    ".ExplicitAndSkip failed: Unsafe case-skipping predicate threw!",
                    ".Fail failed: Unsafe case-skipping predicate threw!",
                    ".Pass failed: Unsafe case-skipping predicate threw!"));
        }

        public void ShouldFailCaseWhenSkipReasonThrows()
        {
            Convention.CaseExecution
                .Skip(HasSkipAttribute, @case => throw new Exception("Unsafe case-skipped reason generator threw!"));

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                For<SkippedTestClass>(
                    ".Explicit passed",
                    ".ExplicitAndSkip failed: Unsafe case-skipped reason generator threw!",
                    ".Fail failed: Unsafe case-skipped reason generator threw!",
                    ".Pass passed"));
        }

        static string ExplicitAttributeReason(MethodInfo testMethod)
        {
            return "[Explicit] tests run only when they are individually selected for execution.";
        }

        static bool HasExplicitAttribute(MethodInfo testMethod)
        {
            return testMethod.HasOrInherits<ExplicitAttribute>();
        }

        static string SkipAttributeReason(MethodInfo testMethod)
        {
            var skip = testMethod.HasOrInherits<SkipAttribute>()
                ? testMethod.GetCustomAttribute<SkipAttribute>(true)
                : testMethod.DeclaringType.GetCustomAttribute<SkipAttribute>(true);

            return skip.Reason;
        }

        static bool HasSkipAttribute(MethodInfo testMethod)
        {
            return testMethod.HasOrInherits<SkipAttribute>();
        }

        class SkippedTestClass
        {
            [Skip("Troublesome test skipped.")]
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
    }
}