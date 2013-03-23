using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class ClassFixture : Fixture
    {
        readonly Type fixtureClass;
        readonly Convention convention;

        public ClassFixture(Type fixtureClass, Convention convention)
        {
            this.fixtureClass = fixtureClass;
            this.convention = convention;
        }

        public ClassFixture(Type fixtureClass, Convention convention, object initialInstance)
        {
            this.fixtureClass = fixtureClass;
            this.convention = convention;

            Instance = initialInstance;
        }

        public object Instance { get; private set; }

        public string Name
        {
            get { return fixtureClass.FullName; }
        }

        public Result Execute(Listener listener)
        {
            var result = new Result();

            foreach (var @case in Cases)
                result = Result.Combine(result, Execute(@case, listener));

            return result;
        }

        private IEnumerable<Case> Cases
        {
            get { return convention.CaseMethods(fixtureClass).Select(method => new MethodCase(this, method)); }
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