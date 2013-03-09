using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class DefaultConfiguration : Configuration
    {
        readonly Type[] candidateTypes;

        public DefaultConfiguration(Assembly assembly)
            : this(assembly.GetTypes()) { }

        public DefaultConfiguration(params Type[] candidateTypes)
        {
            this.candidateTypes = candidateTypes;
        }

        public IEnumerable<Fixture> Fixtures
        {
            get
            {
                return candidateTypes
                    .Where(type =>
                           type.IsClass &&
                           !type.IsAbstract &&
                           type.Name.EndsWith("Tests") &&
                           type.GetConstructor(Type.EmptyTypes) != null)
                    .Select(fixtureClass => new ClassFixture(fixtureClass));
            }
        }
    }
}