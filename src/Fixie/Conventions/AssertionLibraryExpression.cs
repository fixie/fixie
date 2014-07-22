using System;

namespace Fixie.Conventions
{
    public class AssertionLibraryExpression
    {
        readonly Configuration config;

        public AssertionLibraryExpression(Configuration config)
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