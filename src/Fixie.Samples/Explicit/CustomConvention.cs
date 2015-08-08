namespace Fixie.Samples.Explicit
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            ClassExecution
                .CreateInstancePerClass();

            CaseExecution
                .Skip(SkipDueToExplicitAttribute,
                      @case => "[Explicit] tests run only when they are individually selected for execution.");
        }

        bool SkipDueToExplicitAttribute(Case @case)
        {
            var method = @case.Method;

            var isMarkedExplicit = method.Has<ExplicitAttribute>();

            return isMarkedExplicit && TargetMember != method;
        }
    }
}