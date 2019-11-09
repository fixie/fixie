namespace Fixie
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// The context in which a test class is running.
    /// </summary>
    public class TestClass
    {
        readonly Action<Action<Case>> runCases;
        readonly bool isStatic;
        internal TestClass(Type type, Action<Action<Case>> runCases, ConstructorInfo constructorInfo, object[] constructorParameters) : this(type, runCases, constructorInfo, constructorParameters, null) { }
        internal TestClass(Type type, Action<Action<Case>> runCases, ConstructorInfo constructorInfo, object[] constructorParameters, MethodInfo targetMethod)
        {
            this.runCases = runCases;
            Type = type;
            ConstructorInfo = constructorInfo;
            TargetMethod = targetMethod;
            isStatic = Type.IsStatic();
            ConstructorParameters = constructorParameters;
        }

        /// <summary>
        /// The test class to execute.
        /// </summary>
        public Type Type { get; }

        public ConstructorInfo ConstructorInfo { get; }

        /// <summary>
        /// For parameterized test cases, gets the set of parameters to be passed into the test class.
        /// For zero-argument test classes, this property is null.
        /// </summary>
        public object[] ConstructorParameters { get; }

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
                if (ConstructorParameters.Length == 0)
                {
                    return Activator.CreateInstance(Type);
                }

                return Activator.CreateInstance(Type, ConstructorParameters);
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