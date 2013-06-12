using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public static class MethodFilterExtensions
    {
        public static ExceptionList InvokeAll(this MethodFilter methodFilter, Type type, object instance)
        {
            var invoke = new Invoke();
            var exceptions = new ExceptionList();

            foreach (var method in methodFilter.Filter(type))
                invoke.Execute(method, instance, exceptions);

            return exceptions;
        }
    }
}