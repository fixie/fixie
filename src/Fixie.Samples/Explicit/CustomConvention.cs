using Fixie.Conventions;

namespace Fixie.Samples.Explicit
{
    public class CustomConvention : Convention
    {
        public CustomConvention(RunContext runContext)
        {
            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Cases
                .Where(method => method.Void())
                .Where(method =>
                {
                    var isMarkedExplicit = method.Has<ExplicitAttribute>();

                    return !isMarkedExplicit || runContext.TargetMember == method;
                });

            ClassExecution
                .CreateInstancePerTestClass();
        }
    }
}