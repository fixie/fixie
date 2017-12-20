#if NET471
namespace Fixie.Tests.Execution
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Runtime.Versioning;
    using Assertions;

    public class ExecutionEnvironmentTests
    {
        public void ShouldEnableAccessToTestAssemblyConfigFile()
        {
            ConfigurationManager.AppSettings["CanAccessAppConfig"].ShouldEqual("true");
        }
    }
}
#endif