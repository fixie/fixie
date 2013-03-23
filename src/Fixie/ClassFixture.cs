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

        public ClassFixture(Type fixtureClass, object initialInstance)
        {
            this.fixtureClass = fixtureClass;
            Instance = initialInstance;
        }

        public object Instance { get; private set; }

        public string Name
        {
            get { return fixtureClass.FullName; }
        }

        public IEnumerable<Case> Cases
        {
            get
            {
                return CaseMethods(fixtureClass)
                                   .Select(method => new MethodCase(this, method));
            }
        }

        IEnumerable<MethodInfo> CaseMethods(Type fixtureClass)
        {
            return fixtureClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(method =>
                                      method.ReturnType == typeof(void) &&
                                      method.GetParameters().Length == 0 &&
                                      method.DeclaringType != typeof(object));
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
            Instance = null;

            try
            {
                try
                {
                    Instance = Activator.CreateInstance(fixtureClass);
                }
                catch (TargetInvocationException ex)
                {
                    listener.CaseFailed(@case, ex.InnerException);
                    return Result.Fail;
                }
                catch (Exception ex)
                {
                    listener.CaseFailed(@case, ex);
                    return Result.Fail;
                }

                return @case.Execute(listener);
            }
            finally
            {
                Instance = null;
            }
        }
    }
}