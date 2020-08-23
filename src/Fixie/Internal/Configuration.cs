namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class Configuration
    {
        readonly List<Func<Type, bool>> testClassConditions;
        readonly List<Func<MethodInfo, bool>> testMethodConditions;
        bool usingDefaultTestClassCondition;

        public Configuration()
        {
            OrderMethods = methods => methods;

            usingDefaultTestClassCondition = true;
            testClassConditions = new List<Func<Type, bool>>
            {
                x => x.Name.EndsWith("Tests")
            };
            testMethodConditions = new List<Func<MethodInfo, bool>>();
        }

        public Func<IReadOnlyList<MethodInfo>, IReadOnlyList<MethodInfo>> OrderMethods { get; set; }

        public void AddTestClassCondition(Func<Type, bool> testClassCondition)
        {
            if (usingDefaultTestClassCondition)
            {
                //The default test class condition is useful, but too restrictive
                //in the case that a customization is defining their own test class
                //discovery rules. Upon the first such customization, we start over
                //from an empty list of conditions.

                testClassConditions.Clear();
            }

            testClassConditions.Add(testClassCondition);
            usingDefaultTestClassCondition = false;
        }

        public void AddTestMethodCondition(Func<MethodInfo, bool> testMethodCondition)
            => testMethodConditions.Add(testMethodCondition);

        public IReadOnlyList<Func<Type, bool>> TestClassConditions => testClassConditions;
        public IReadOnlyList<Func<MethodInfo, bool>> TestMethodConditions => testMethodConditions;
    }
}