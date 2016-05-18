namespace Fixie.Conventions
{
    using System;
    using Internal;

    public class AssertionLibraryExpression
    {
        readonly Configuration config;

        internal AssertionLibraryExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Identifies the given type as an implementation detail of an assertion library,
        /// omitting it from test failure stack traces.
        /// </summary>
        public AssertionLibraryExpression For(Type libraryInfrastructureType)
        {
            config.AddAssertionLibraryType(libraryInfrastructureType);
            return this;
        }

        /// <summary>
        /// Identifies the given type as an implementation detail of an assertion library,
        /// omitting it from test failure stack traces.
        /// </summary>
        public AssertionLibraryExpression For<TLibraryInfrastructure>()
        {
            return For(typeof(TLibraryInfrastructure));
        }
    }
}