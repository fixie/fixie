using System;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Tests.TestMethods
{
    public class SkippedCaseTests
    {
        readonly StubListener listener;
        readonly Convention convention;

        public SkippedCaseTests()
        {
            listener = new StubListener();

            convention = new SelfTestConvention();
        }

        public void ShouldSkipCases()
        {
            convention.CaseExecution
                .Skip(HasSkipAttribute);

            convention.Execute(listener, typeof(SkippedTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestMethods.SkippedCaseTests+SkippedTestClass.Fail skipped.",
                "Fixie.Tests.TestMethods.SkippedCaseTests+SkippedTestClass.Pass passed.");
        }

        public void ShouldSkipCasesWithOptionalReason()
        {
            convention.CaseExecution
                .Skip(HasSkipAttribute, SkipAttributeReason);

            convention.Execute(listener, typeof(SkippedTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestMethods.SkippedCaseTests+SkippedTestClass.Fail skipped: Troublesome test skipped.",
                "Fixie.Tests.TestMethods.SkippedCaseTests+SkippedTestClass.Pass passed.");
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