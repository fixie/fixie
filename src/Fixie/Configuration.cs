using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class Configuration
    {
        readonly Type[] candidateTypes;

        public Configuration(Assembly assembly)
            : this(assembly.GetTypes()) { }

        public Configuration(params Type[] candidateTypes)
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