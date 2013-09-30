using Fixie.Conventions;

namespace Fixie.Samples.ChangeCaseName
{
    public class CustomConvention : Convention
    {
        public CustomConvention(RunContext runContext)
        {
            CaseBuilder = new CustomCaseBuilder();
            
            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            ClassExecution
                .CreateInstancePerTestClass();
        }
    }
}