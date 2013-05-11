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

        public object Instance { get; private set; }

        public string Name
        {
            get { return fixtureClass.FullName; }
        }

        public void Execute(Listener listener)
        {
            foreach (var @case in Cases)
                Execute(@case, listener);
        }

        IEnumerable<Case> Cases
        {
            get { return convention.CaseMethods(fixtureClass).Select(method => new MethodCase(this, method)); }
        }

        void Execute(Case @case, Listener listener)
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
                    listener.CaseFailed(@case, new[] { ex.InnerException });
                    return;
                }
                catch (Exception ex)
                {
                    listener.CaseFailed(@case, new[] { ex });
                    return;
                }

                var exceptions = new List<Exception>();

                try
                {
                    @case.Execute(listener, exceptions);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                try
                {
                    var disposable = Instance as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                if (exceptions.Any())
                    listener.CaseFailed(@case, exceptions.ToArray());
                else
                    listener.CasePassed(@case);
            }
            finally
            {
                Instance = null;
            }
        }
    }
}