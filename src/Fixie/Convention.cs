using Fixie.Conventions;
using Fixie.Internal;

namespace Fixie
{
    public class Convention
    {
        public Convention()
        {
            Config = new Configuration();

            Classes = new ClassExpression(Config);
            Methods = new MethodExpression(Config);;
            Parameters = new ParameterSourceExpression(Config);
            CaseExecution = new CaseBehaviorExpression(Config);
            FixtureExecution = new FixtureBehaviorExpression(Config);
            ClassExecution = new ClassBehaviorExpression(Config);
            HideExceptionDetails = new AssertionLibraryExpression(Config);
        }

        public Configuration Config { get; private set; }

        public ClassExpression Classes { get; private set; }
        public MethodExpression Methods { get; private set; }
        public ParameterSourceExpression Parameters { get; private set; }

        public CaseBehaviorExpression CaseExecution { get; private set; }
        public FixtureBehaviorExpression FixtureExecution { get; private set; }
        public ClassBehaviorExpression ClassExecution { get; private set; }
        public AssertionLibraryExpression HideExceptionDetails { get; private set; }
    }
}