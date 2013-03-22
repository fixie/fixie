using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class ClassFixture : Fixture
    {
        readonly Type fixtureClass;

        public ClassFixture(Type fixtureClass)
        {
            this.fixtureClass = fixtureClass;
        }

        public object GetInstance()
        {
            return Activator.CreateInstance(fixtureClass);
        }

        public string Name
        {
            get { return fixtureClass.FullName; }
        }

        public IEnumerable<Case> Cases
        {
            get
            {
                return fixtureClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                   .Where(method =>
                                          method.ReturnType == typeof(void) &&
                                          method.GetParameters().Length == 0 &&
                                          method.DeclaringType != typeof(object))
                                   .Select(method => new MethodCase(this, method));
            }
        }

        public Result Execute(Listener listener)
        {
            var result = new Result();

            foreach (var @case in Cases)
                result = Result.Combine(result, Execute(@case, listener));

            return result;
        }

        Result Execute(Case @case, Listener listener)
        {
            try
            {
                return @case.Execute(listener);
            }
            catch (Exception ex)
            {
                listener.CaseFailed(@case, ex);
                return Result.Fail;
            }
        }
    }
}