namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Implement Discovery to customize test discovery rules.
    /// </summary>
    public interface Discovery
    {
        /// <summary>
        /// Filters a set of candidate classes to those which are to be treated as test classes.
        /// </summary>
        IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses);

        /// <summary>
        /// Filters a set of candidate methods to those which are to be treated as test methods.
        /// </summary>
        IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods);
    }
}