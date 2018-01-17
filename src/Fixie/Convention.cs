namespace Fixie
{
    using System;
    using System.Reflection;
    using Cli;
    using Conventions;

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
            ClassExecution = new ClassBehaviorExpression(Config);
            HideExceptionDetails = new AssertionLibraryExpression(Config);
        }

        /// <summary>
        /// The current state describing the convention. This state can be manipulated through
        /// the other properties on Convention.
        /// </summary>
        internal Configuration Config { get; }

        /// <summary>
        /// Gets the target MethodInfo identified by the test runner as the sole item
        /// to be executed. Null under normal test execution.
        /// </summary>
        public MethodInfo TargetMethod => RunContext.TargetMethod;

        /// <summary>
        /// Defines the set of conditions that describe which classes are test classes.
        /// </summary>
        public ClassExpression Classes { get; }

        /// <summary>
        /// Defines the set of conditions that describe which test class methods are test methods.
        /// </summary>
        public MethodExpression Methods { get; }

        /// <summary>
        /// Defines the set of parameter sources, which provide inputs to parameterized test methods.
        /// </summary>
        public ParameterSourceExpression Parameters { get; }

        /// <summary>
        /// Customizes the execution of each test case.
        /// </summary>
        public CaseBehaviorExpression CaseExecution { get; }

        /// <summary>
        /// Customizes the execution of each test class.
        /// </summary>
        public ClassBehaviorExpression ClassExecution { get; }

        /// <summary>
        /// Defines the set of types which make up an assertion library, so that test case failure stack
        /// traces can be simplified for readability.
        /// </summary>
        public AssertionLibraryExpression HideExceptionDetails { get; }
    }
}