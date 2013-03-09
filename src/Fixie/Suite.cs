using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class Suite
    {
        readonly Type[] candidateTypes;

        public Suite(Assembly assembly)
            : this(assembly.GetTypes()) { }

        public Suite(params Type[] candidateTypes)
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