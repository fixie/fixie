namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Implement Discovery to customize test discovery rules.
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
    public interface Discovery
    {
        /// <summary>
        /// Filters a set of candidate classes to those which are to be treated as test classes.
        /// </summary>
        IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses)
            => concreteClasses.Where(x => x.Name.EndsWith("Tests"));

        /// <summary>
        /// Filters a set of candidate methods to those which are to be treated as test methods.
        /// </summary>
        IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
            => publicMethods;
    }
}