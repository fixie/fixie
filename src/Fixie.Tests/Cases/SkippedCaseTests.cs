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
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.ExplicitAndSkip skipped",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail skipped",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Explicit passed",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed");
        }

        public void ShouldSkipCasesWithOptionalReason()
        {
            Convention.CaseExecution
                .Skip(HasSkipAttribute, SkipAttributeReason);

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.ExplicitAndSkip skipped",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail skipped: Troublesome test skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Explicit passed",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed");
        }

        public void ShouldAllowMultiplePrioritizedSkipBehaviors()
        {
            Convention.CaseExecution
                .Skip(HasExplicitAttribute, ExplicitAttributeReason)
                .Skip(HasSkipAttribute, SkipAttributeReason);

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Explicit skipped: [Explicit] tests run only when they are individually selected for execution.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.ExplicitAndSkip skipped: [Explicit] tests run only when they are individually selected for execution.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail skipped: Troublesome test skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed");
        }

        public void ShouldAllowSkippingViaBehaviorTypes()
        {
            Convention.CaseExecution
                .Skip<SkipByExplicitAttribute>()
                .Skip<SkipBySkipAttribute>();

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Explicit skipped: [Explicit] tests run only when they are individually selected for execution.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.ExplicitAndSkip skipped: [Explicit] tests run only when they are individually selected for execution.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail skipped: Troublesome test skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed");
        }

        public void ShouldAllowSkippingViaBehaviorInstances()
        {
            Convention.CaseExecution
                .Skip(new SkipByExplicitAttribute())
                .Skip(new SkipBySkipAttribute());

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Explicit skipped: [Explicit] tests run only when they are individually selected for execution.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.ExplicitAndSkip skipped: [Explicit] tests run only when they are individually selected for execution.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail skipped: Troublesome test skipped.",
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed");
        }

        public void ShouldFailCaseWithClearExplanationWhenSkipConditionThrows()
        {
            Convention.CaseExecution
                .Skip(@case => { throw new Exception("Unsafe case-skipping predicate threw!"); });

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Explicit failed: Exception thrown while attempting to run a custom case-skipping predicate. Check the inner exception for more details." + Environment.NewLine +
                "    Inner Exception: Unsafe case-skipping predicate threw!",

                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.ExplicitAndSkip failed: Exception thrown while attempting to run a custom case-skipping predicate. Check the inner exception for more details." + Environment.NewLine +
                "    Inner Exception: Unsafe case-skipping predicate threw!",

                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail failed: Exception thrown while attempting to run a custom case-skipping predicate. Check the inner exception for more details." + Environment.NewLine +
                "    Inner Exception: Unsafe case-skipping predicate threw!",

                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass failed: Exception thrown while attempting to run a custom case-skipping predicate. Check the inner exception for more details." + Environment.NewLine +
                "    Inner Exception: Unsafe case-skipping predicate threw!");
        }

        public void ShouldFailCaseWithClearExplanationWhenSkipReasonThrows()
        {
            Convention.CaseExecution
                .Skip(HasSkipAttribute, @case => { throw new Exception("Unsafe case-skipped reason generator threw!"); });

            Run<SkippedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.ExplicitAndSkip failed: Exception thrown while attempting to get a custom case-skipped reason. Check the inner exception for more details." + Environment.NewLine +
                "    Inner Exception: Unsafe case-skipped reason generator threw!",

                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Fail failed: Exception thrown while attempting to get a custom case-skipped reason. Check the inner exception for more details." + Environment.NewLine +
                "    Inner Exception: Unsafe case-skipped reason generator threw!",

                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Explicit passed",

                "Fixie.Tests.Cases.SkippedCaseTests+SkippedTestClass.Pass passed");
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

        class SkipByExplicitAttribute : SkipBehavior
        {
            public override bool SkipCase(Case @case) => HasExplicitAttribute(@case);
            public override string GetSkipReason(Case @case) => ExplicitAttributeReason(@case);
        }

        class SkipBySkipAttribute : SkipBehavior
        {
            public override bool SkipCase(Case @case) => HasSkipAttribute(@case);
            public override string GetSkipReason(Case @case) => SkipAttributeReason(@case);
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