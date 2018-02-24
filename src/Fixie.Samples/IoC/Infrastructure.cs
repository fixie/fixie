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

    public class IoCContainer : IDisposable
    {
        readonly Dictionary<Type, Type> typeMappings = new Dictionary<Type, Type>();
        readonly List<object> instances = new List<object>();

        public void Add(Type requestedType, Type concreteType)
        {
            typeMappings[requestedType] = concreteType;
        }

        public object Get(Type type)
        {
            if (typeMappings.ContainsKey(type))
                type = typeMappings[type];

            var constructor = type.GetConstructors().Single();

            var parameters = constructor.GetParameters();

            var arguments = parameters.Select(p => Get(p.ParameterType)).ToArray();

            var instance = Activator.CreateInstance(type, arguments);

            instances.Add(instance);

            return instance;
        }

        public void Dispose()
        {
            foreach (var instance in instances)
                instance.Dispose();
        }
    }
}
