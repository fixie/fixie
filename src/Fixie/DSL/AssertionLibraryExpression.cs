using System;
using Fixie.Conventions;

namespace Fixie.DSL
{
    public class AssertionLibraryExpression
    {
        readonly ConfigModel config;

        public AssertionLibraryExpression(ConfigModel config)
        {
            this.config = config;
        }

        public AssertionLibraryExpression For(Type libraryInfrastructureType)
        {
            config.AddAssertionLibraryType(libraryInfrastructureType);
            return this;
        }

        public AssertionLibraryExpression For<TLibraryInfrastructure>()
        {
            return For(typeof(TLibraryInfrastructure));
        }
    }
}