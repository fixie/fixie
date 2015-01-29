using System.Reflection;
using Fixie.Conventions;
using Fixie.Internal;

namespace Fixie
{
    /// <summary>
    /// Base class for all Fixie conventions.  Subclass Convention to customize test discovery and execution.
    /// </summary>
    public class Convention
    {
        public Convention()
        {
            Config = new Configuration();

            Classes = new ClassExpression(Config);
            Methods = new MethodExpression(Config);
            Parameters = new ParameterSourceExpression(Config);
            CaseExecution = new CaseBehaviorExpression(Config);
            FixtureExecution = new FixtureBehaviorExpression(Config);
            ClassExecution = new ClassBehaviorExpression(Config);
            HideExceptionDetails = new AssertionLibraryExpression(Config);
        }

        /// <summary>
        /// The current state describing the convention. This state can be manipulated through
        /// the other properties on Convention.
        /// </summary>
        public Configuration Config { get; private set; }

        /// <summary>
        /// Gets the custom Options set provided by the test runner at the start of execution.
        /// </summary>
        public Options Options { get { return RunContext.Options; } }

        /// <summary>
        /// Gets the target Type or MethodInfo identified by the test runner as the sole item
        /// to be executed. Null under normal test execution.
        /// </summary>
        public MemberInfo TargetMember { get { return RunContext.TargetMember; } }

        /// <summary>
        /// Defines the set of conditions that describe which classes are test classes.
        /// </summary>
        public ClassExpression Classes { get; private set; }

        /// <summary>
        /// Defines the set of conditions that describe which test class methods are test methods.
        /// </summary>
        public MethodExpression Methods { get; private set; }

        /// <summary>
        /// Defines the set of parameter sources, which provide inputs to parameterized test methods.
        /// </summary>
        public ParameterSourceExpression Parameters { get; private set; }

        /// <summary>
        /// Customizes the execution of each test case.
        /// </summary>
        public CaseBehaviorExpression CaseExecution { get; private set; }

        /// <summary>
        /// Customizes the execution of each test fixture (test class instance).
        /// </summary>
        public FixtureBehaviorExpression FixtureExecution { get; private set; }

        /// <summary>
        /// Customizes the execution of each test class.
        /// </summary>
        public ClassBehaviorExpression ClassExecution { get; private set; }

        /// <summary>
        /// Defines the set of types which make up an assertion library, so that test case failure stack
        /// traces can be simplified for readability.
        /// </summary>
        public AssertionLibraryExpression HideExceptionDetails { get; private set; }
    }
}