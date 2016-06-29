namespace Fixie.Samples.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IThirdPartyService
    {
        string Invoke();
    }

    public class RealThirdPartyService : IThirdPartyService
    {
        public string Invoke() => nameof(RealThirdPartyService);
    }

    public class FakeThirdPartyService : IThirdPartyService
    {
        public string Invoke() => nameof(FakeThirdPartyService);
    }

    public interface IDatabase
    {
        string Query();
    }

    public class RealDatabase : IDatabase
    {
        public string Query() => nameof(RealDatabase);
    }

    public class FakeDatabase : IDatabase
    {
        public string Query() => nameof(FakeDatabase);
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
