using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Samples.MbUnitStyle
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .HasOrInherits<TestFixtureAttribute>();

            Methods
                .HasOrInherits<TestAttribute>();

            ClassExecution
                    .CreateInstancePerClass()
                    .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));

            FixtureExecution
                .Wrap<FixtureSetUpTearDown>();

            CaseExecution
                .Wrap<SupportExpectedExceptions>()
                .Wrap<SetUpTearDown>();

            Parameters
                .Add<RowAttributeParameterSource>()
                .Add<ColumnAttributeParameterSource>();
        }

        class RowAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                ParameterInfo[] parameterInfos = method.GetParameters();

                var rowAttributes = method.GetCustomAttributes<RowAttribute>(true);

                foreach (var rowAttribute in rowAttributes)
                {
                    object[] parameters = rowAttribute.Parameters;

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        parameters[i] = ChangeType(parameters[i], parameterInfos[i].ParameterType);
                    }
                
                    yield return parameters;
                }
            }
        }

        class ColumnAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return CartesianProduct(Columns(method));
            }

            static IEnumerable<object[]> Columns(MethodInfo method)
            {
                ParameterInfo[] parameterInfos = method.GetParameters();

                if (parameterInfos.Length == 0)
                    return null;

                if (parameterInfos[0].GetCustomAttributes<ColumnAttribute>(true).Any() == false)
                    return null;

                return GetColumnParameters(parameterInfos);
            }

            private static IEnumerable<object[]> GetColumnParameters(ParameterInfo[] parameterInfos)
            {
                foreach (var parameterInfo in parameterInfos)
                {
                    yield return Array.ConvertAll(parameterInfo.GetCustomAttributes<ColumnAttribute>(true).Single().Parameters,
                                                  x => ChangeType(x, parameterInfo.ParameterType));
                }
            }

            static IEnumerable<object[]> CartesianProduct(IEnumerable<object[]> sequences)
            {
                if (sequences == null)
                    return new object[][] { };

                //See http://blogs.msdn.com/b/ericlippert/archive/2010/06/28/computing-a-cartesian-product-with-linq.aspx

                IEnumerable<object[]> emptyProduct = new[] { new object[] { } };
                
                return sequences.Aggregate(
                    emptyProduct,
                    (accumulator, sequence) =>
                        from accseq in accumulator
                        from item in sequence
                        select accseq.Concat(new[] { item }).ToArray());
            }
        }

        private static object ChangeType(object parameter, Type type)
        {
            if (parameter != null && parameter.GetType() != type)
            {
                try
                {
                    parameter = Convert.ChangeType(parameter, type);
                }
                catch { }
            }

            return parameter;
        }
    }

    class SupportExpectedExceptions : CaseBehavior
    {
        public void Execute(Case @case, Action next)
        {
            next();

            var attribute = @case.Method.GetCustomAttributes<ExpectedExceptionAttribute>(false).SingleOrDefault();

            if (attribute == null)
                return;

            if (@case.Exceptions.Count > 1)
                return;

            var exception = @case.Exceptions.SingleOrDefault();

            if (exception == null)
                throw new Exception("Expected exception of type " + attribute.ExpectedException + ".");

            if (!attribute.ExpectedException.IsAssignableFrom(exception.GetType()))
            {
                @case.ClearExceptions();

                throw new Exception("Expected exception of type " + attribute.ExpectedException + " but an exception of type " + exception.GetType() + " was thrown.", exception);
            }

            if (attribute.ExpectedMessage != null && exception.Message != attribute.ExpectedMessage)
            {
                @case.ClearExceptions();

                throw new Exception("Expected exception message '" + attribute.ExpectedMessage + "'" + " but was '" + exception.Message + "'.", exception);
            }

            @case.ClearExceptions();
        }
    }

    class SetUpTearDown : CaseBehavior
    {
        public void Execute(Case @case, Action next)
        {
            @case.Class.InvokeAll<SetUpAttribute>(@case.Fixture.Instance);
            next();
            @case.Class.InvokeAll<TearDownAttribute>(@case.Fixture.Instance);
        }
    }

    class FixtureSetUpTearDown : FixtureBehavior
    {
        public void Execute(Fixture fixture, Action next)
        {
            fixture.Class.Type.InvokeAll<FixtureSetUpAttribute>(fixture.Instance);
            next();
            fixture.Class.Type.InvokeAll<FixtureTearDownAttribute>(fixture.Instance);
        }
    }

    public static class BehaviorBuilderExtensions
    {
        public static void InvokeAll<TAttribute>(this Type type, object instance)
            where TAttribute : Attribute
        {
            foreach (var method in Has<TAttribute>(type))
            {
                try
                {
                    method.Invoke(instance, null);
                }
                catch (TargetInvocationException exception)
                {
                    throw new PreservedException(exception.InnerException);
                }
            }
        }

        static IEnumerable<MethodInfo> Has<TAttribute>(Type type) where TAttribute : Attribute
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.HasOrInherits<TAttribute>());
        }
    }
}
