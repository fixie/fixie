using System;
using System.Reflection;

namespace Fixie.Conventions
{
    public class ConfigModel
    {
        public ConfigModel()
        {
            ConstructionFrequency = ConstructionFrequency.PerCase;
            Factory = UseDefaultConstructor;
        }

        public Func<Type, object> Factory { get; set; }

        public ConstructionFrequency ConstructionFrequency { get; set; }

        static object UseDefaultConstructor(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception.InnerException);
            }
        }
    }
}