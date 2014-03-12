using System;
using System.Reflection;

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
    }
}