namespace Fixie
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Internal;

    /// <summary>
    /// The context in which a test class is running.
    /// </summary>
    public class TestClass
    {
        readonly Action<Action<Case>> runCases;
        readonly bool isStatic;
        internal TestClass(Type type, Action<Action<Case>> runCases) : this(type, runCases, null) { }

        internal TestClass(Type type, Action<Action<Case>> runCases, MethodInfo targetMethod)
        {
            this.runCases = runCases;
            Type = type;
            TargetMethod = targetMethod;
            isStatic = Type.IsStatic();
        }

        /// <summary>
        /// The test class to execute.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the target MethodInfo identified by the
        /// test runner as the sole method to be executed.
        /// Null under normal test execution.
        /// </summary>
        public MethodInfo TargetMethod { get; }

        /// <summary>
        /// Constructs an instance of the test class type, using its default constructor.
        /// If the class is static, no action is taken and null is returned.
        /// </summary>
        public object Construct()
        {
            if (isStatic)
                return null;

            try
            {
                return Activator.CreateInstance(Type);
            }
            catch (TargetInvocationException exception)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw; //Unreachable.
            }
        }

        public void RunCases(Action<Case> caseLifecycle)
        {
            runCases(caseLifecycle);
        }
    }
}