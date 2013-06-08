using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Samples.IoC
{
    public interface IThirdPartyService
    {
        string Invoke();
    }

    public class RealThirdPartyService : IThirdPartyService
    {
        public string Invoke()
        {
            return typeof(RealThirdPartyService).Name;
        }
    }

    public class FakeThirdPartyService : IThirdPartyService
    {
        public string Invoke()
        {
            return typeof(FakeThirdPartyService).Name;
        }
    }

    public interface IDatabase
    {
        string Query();
    }

    public class RealDatabase : IDatabase
    {
        public string Query()
        {
            return typeof(RealDatabase).Name;
        }
    }

    public class FakeDatabase : IDatabase
    {
        public string Query()
        {
            return typeof(FakeDatabase).Name;
        }
    }

    public class IoCContainer
    {
        readonly Dictionary<Type, object> instances = new Dictionary<Type, object>();

        public void Add(Type type, object instance)
        {
            instances[type] = instance;
        }

        public object Get(Type type)
        {
            if (instances.ContainsKey(type))
                return instances[type];

            var constructor = type.GetConstructors().Single();

            var parameters = constructor.GetParameters();

            var arguments = parameters.Select(p => Get(p.ParameterType)).ToArray();

            return Activator.CreateInstance(type, arguments);
        }
    }
}
