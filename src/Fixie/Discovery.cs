namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

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
        /// <summary>
        /// Defines the set of conditions that describe which classes are test classes.
        /// </summary>
        public List<Func<Type, bool>> TestClassConditions { get; } = new List<Func<Type, bool>>();

        /// <summary>
        /// Defines the set of conditions that describe which test class methods are test methods,
        /// and what order to run them in.
        /// </summary>
        public List<Func<MethodInfo, bool>> TestMethodConditions { get; } = new List<Func<MethodInfo, bool>>();
    }
}