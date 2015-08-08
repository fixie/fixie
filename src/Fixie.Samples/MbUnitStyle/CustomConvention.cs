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
                return method.GetCustomAttributes<RowAttribute>(true).Select(input => input.Parameters);
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
                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length == 0)
                    return null;

                if (parameters[0].GetCustomAttributes<ColumnAttribute>(true).Any() == false)
                    return null;

                return parameters
                    .Select(parameter =>
                        parameter.GetCustomAttributes<ColumnAttribute>(true).Single().Parameters);
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

            if (exception.GetType() != attribute.ExpectedException)
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
