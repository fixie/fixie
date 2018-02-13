namespace Fixie.Tests.Cases
{
    using System;
    using System.Reflection;
    using static System.Environment;

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

        public void ShouldAllowSkippingViaBehaviorTypes()
        {
            Convention.CaseExecution
                .Skip<SkipByExplicitAttribute>()
                .Skip<SkipBySkipAttribute>();

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                For<SkippedTestClass>(
                    ".Explicit skipped: [Explicit] tests run only when they are individually selected for execution.",
                    ".ExplicitAndSkip skipped: [Explicit] tests run only when they are individually selected for execution.",
                    ".Fail skipped: Troublesome test skipped.",
                    ".Pass passed"));
        }

        public void ShouldAllowSkippingViaBehaviorInstances()
        {
            Convention.CaseExecution
                .Skip(new SkipByExplicitAttribute())
                .Skip(new SkipBySkipAttribute());

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

            var skip = method.HasOrInherits<SkipAttribute>()
                ? method.GetCustomAttribute<SkipAttribute>(true)
                : method.DeclaringType.GetCustomAttribute<SkipAttribute>(true);

            return skip.Reason;
        }

        static bool HasSkipAttribute(Case @case)
        {
            return @case.Method.HasOrInherits<SkipAttribute>();
        }

        class SkipByExplicitAttribute : SkipBehavior
        {
            public bool SkipCase(Case @case) => HasExplicitAttribute(@case);
            public string GetSkipReason(Case @case) => ExplicitAttributeReason(@case);
        }

        class SkipBySkipAttribute : SkipBehavior
        {
            public bool SkipCase(Case @case) => HasSkipAttribute(@case);
            public string GetSkipReason(Case @case) => SkipAttributeReason(@case);
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