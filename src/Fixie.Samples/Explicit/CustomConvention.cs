namespace Fixie.Samples.Explicit
{
    public class CustomConvention : Convention
    {
        public CustomConvention(RunContext runContext)
        {
            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid())
                .Where(method =>
                {
                    var isMarkedExplicit = method.Has<ExplicitAttribute>();

                    return !isMarkedExplicit || runContext.TargetMember == method;
                });

            ClassExecution
                .CreateInstancePerClass();
        }
    }
}