namespace Fixie.Assertions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    class AssertEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            var type = typeof(T);

            // Null?
            if (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
            {
                if (object.Equals(x, default(T)))
                    return object.Equals(y, default(T));

                if (object.Equals(y, default(T)))
                    return false;
            }

            //x implements IEquitable<T> and is assignable from y?
            var xIsAssignableFromY = x.GetType().IsInstanceOfType(y);
            if (xIsAssignableFromY && x is IEquatable<T> equatable1)
                return equatable1.Equals(y);

            //y implements IEquitable<T> and is assignable from x?
            var yIsAssignableFromX = y.GetType().IsInstanceOfType(x);
            if (yIsAssignableFromX && y is IEquatable<T> equatable2)
                return equatable2.Equals(x);

            // Enumerable?
            if (x is IEnumerable enumerableX && y is IEnumerable enumerableY)
                return new EnumerableEqualityComparer().Equals(enumerableX, enumerableY);

            // Last case, rely on object.Equals
            return object.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }
    }
}