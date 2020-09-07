namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Internal.Expressions;

    /// <summary>
    /// Subclass Discovery to customize test discovery rules.
    /// 
    /// The default discovery rules are applied to a test assembly whenever the test
    /// assembly includes no such subclass.
    ///
    /// By default,
    /// 
    /// <para>A class is a test class if its name ends with "Tests".</para>
    ///
    /// <para>All public methods in a test class are test methods.</para>
    /// </summary>
    public class Discovery
    {
        readonly List<Func<Type, bool>> testClassConditions;
        readonly List<Func<MethodInfo, bool>> testMethodConditions;

        public Discovery()
        {
            testClassConditions = new List<Func<Type, bool>>();
            testMethodConditions = new List<Func<MethodInfo, bool>>();

            Classes = new ClassExpression(this);
            Methods = new MethodExpression(this);
        }

        /// <summary>
        /// Defines the set of conditions that describe which classes are test classes.
        /// </summary>
        public ClassExpression Classes { get; }

        /// <summary>
        /// Defines the set of conditions that describe which test class methods are test methods,
        /// and what order to run them in.
        /// </summary>
        public MethodExpression Methods { get; }

        internal void AddTestClassCondition(Func<Type, bool> testClassCondition)
            => testClassConditions.Add(testClassCondition);

        internal void AddTestMethodCondition(Func<MethodInfo, bool> testMethodCondition)
            => testMethodConditions.Add(testMethodCondition);

        internal IReadOnlyList<Func<Type, bool>> TestClassConditions => testClassConditions;
        internal IReadOnlyList<Func<MethodInfo, bool>> TestMethodConditions => testMethodConditions;
    }
}