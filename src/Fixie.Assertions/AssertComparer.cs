namespace Fixie.Assertions
{
    using System;
    using System.Collections.Generic;

    class AssertComparer<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            var type = typeof(T);

            // Null?
            if (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
            {
                if (Equals(x, default(T)))
                {
                    return Equals(y, default(T)) ? 0 : 1;
                }

                if (Equals(y, default(T)))
                    return -1;
            }

            bool xIsAssignableFromY = x.GetType().IsInstanceOfType(y);
            bool yIsAssignableFromX = y.GetType().IsInstanceOfType(x);

            if (!xIsAssignableFromY && !yIsAssignableFromX)
                throw new InvalidOperationException($"Cannot compare objects of type {x.GetType().Name} and {y.GetType().Name} because neither is assignable from the other.");

            // x Implements IComparable<T>?
            if (x is IComparable<T> comparable1 && xIsAssignableFromY)
                return comparable1.CompareTo(y);

            // y Implements IComparable<T>?
            if (y is IComparable<T> comparable2 && yIsAssignableFromX)
                return comparable2.CompareTo(x) * -1;

            // x Implements IComparable?
            if (x is IComparable comparable3 && xIsAssignableFromY)
                return comparable3.CompareTo(y);

            // y Implements IComparable?
            if (y is IComparable comparable4 && yIsAssignableFromX)
                return comparable4.CompareTo(x) *-1;

            if (new AssertEqualityComparer<T>().Equals(x, y))
                return 0;

            if (xIsAssignableFromY)
            {
                var result = CompareUsingOperators(x, y, x.GetType());
                if (result.HasValue)
                    return result.Value;
            }

            if (yIsAssignableFromX)
            {
                var result = CompareUsingOperators(x, y, y.GetType());
                if (result.HasValue)
                    return result.Value;
            }

            throw new InvalidOperationException($"Cannot compare objects of type {x.GetType().Name} and {y.GetType().Name} because neither implements IComparable or IComparable<T> nor overloads comparaison operators.");
        }

        //Note: Handles edge case of a class where operators are overloaded but neither IComparable or IComparable<T> are implemented.
        static int? CompareUsingOperators(T x, T y, Type type)
        {
            var greaterThan = type.GetMethod("op_GreaterThan");
            if (greaterThan != null)
            {
                var lessThan = type.GetMethod("op_LessThan");
                return (bool)greaterThan.Invoke(null, new object[] { x, y })
                    ? 1
                    : (bool)lessThan.Invoke(null, new object[] { x, y }) ? -1 : 0;
            }
            var greaterThanOrEqual = type.GetMethod("op_GreaterThanOrEqual");
            if (greaterThanOrEqual != null)
            {
                var lessThanOrEqual = type.GetMethod("op_LessThanOrEqual");
                return (bool)greaterThanOrEqual.Invoke(null, new object[] { x, y })
                    ? (bool)lessThanOrEqual.Invoke(null, new object[] { x, y }) ? 0 : 1
                    : -1;
            }
            return null;
        }
    }
}