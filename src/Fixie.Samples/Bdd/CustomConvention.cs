using System;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;
using Fixie.Samples.NUnitStyle;

namespace Fixie.Samples.Bdd
{
    public class CustomConvention : Convention
    {
        const BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public CustomConvention()
        {
            Classes.Where(x => x.GetFields(fieldFlags).Any(fieldInfo => fieldInfo.FieldType == typeof(given)));

            Cases = new CustomCaseFilter().SetFilterMethod(x => x.GetFields(fieldFlags).Where(fieldInfo => fieldInfo.FieldType == typeof(then)));

            ClassExecution
                    .CreateInstancePerTestClass();

            InstanceExecution
                .SetUpTearDown(fixture =>
                               {
                                   InvokeFieldsOfType(typeof(given), fixture.TestClass, fixture.Instance);
                                   InvokeFieldsOfType(typeof(when), fixture.TestClass, fixture.Instance);
                               },
                               fixture => InvokeFieldsOfType(typeof(after), fixture.TestClass, fixture.Instance));
        }

        void InvokeFieldsOfType(Type fieldType, Type testType, object instance)
        {
            testType.GetFields(fieldFlags)
                           .Where(x => x.FieldType == fieldType)
                           .ToList()
                           .ForEach(fieldInfo => ((Delegate)fieldInfo.GetValue(instance)).DynamicInvoke());
        }
    }

    public delegate void given();
    public delegate void when();
    public delegate void then();
    public delegate void after();
    
    public static class BehaviorBuilderExtensions
    {
        public static InstanceBehaviorBuilder SetUpTearDown<TSetUpAttribute, TTearDownAttribute>(this InstanceBehaviorBuilder builder)
            where TSetUpAttribute : Attribute
            where TTearDownAttribute : Attribute
        {
            return builder.SetUpTearDown(fixture => InvokeAll<TSetUpAttribute>(fixture.TestClass, fixture.Instance),
                                         fixture => InvokeAll<TTearDownAttribute>(fixture.TestClass, fixture.Instance));
        }

        public static CaseBehaviorBuilder SetUpTearDown<TSetUpAttribute, TTearDownAttribute>(this CaseBehaviorBuilder builder)
            where TSetUpAttribute : Attribute
            where TTearDownAttribute : Attribute
        {
            return builder.SetUpTearDown((@case, instance) => InvokeAll<TSetUpAttribute>(@case.Class, instance),
                                         (@case, instance) => InvokeAll<TTearDownAttribute>(@case.Class, instance));
        }

        static void InvokeAll<TAttribute>(Type type, object instance)
            where TAttribute : Attribute
        {
            foreach (var method in Has<TAttribute>().Filter(type).Select(x => (MethodInfo)x))
            {
                try
                {
                    method.Invoke(instance, null);
                }
                catch (TargetInvocationException ex)
                {
                    throw new PreservedException(ex.InnerException);
                }
            }
        }

        static MethodFilter Has<TAttribute>() where TAttribute : Attribute
        {
            return new MethodFilter().HasOrInherits<TAttribute>();
        }
    }
}