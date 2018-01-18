namespace Fixie.Samples.Skipped
{
    using System;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            CaseExecution
                .Skip(SkipDueToClassLevelSkipAttribute, @case => "Whole class skipped")
                .Skip(SkipDueToMethodLevelSkipAttribute);

            ClassExecution
                .Lifecycle<CreateInstancePerClass>()
                .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));
        }

        static bool SkipDueToClassLevelSkipAttribute(Case @case)
            => @case.Method.DeclaringType.Has<SkipAttribute>();

        static bool SkipDueToMethodLevelSkipAttribute(Case @case)
            => @case.Method.Has<SkipAttribute>();

        class CreateInstancePerClass : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                var instance = Activator.CreateInstance(testClass);

                runCases(@case => @case.Execute(instance));

                (instance as IDisposable)?.Dispose();
            }
        }
    }
}