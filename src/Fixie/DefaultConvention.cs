using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class DefaultConvention : Convention
    {
        readonly Type[] candidateTypes;

        public DefaultConvention(Assembly assembly)
            : this(assembly.GetTypes()) { }

        public DefaultConvention(params Type[] candidateTypes)
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

        public Result Execute(Listener listener)
        {
            var result = new Result();

            foreach (var fixture in Fixtures)
                result = Result.Combine(result, fixture.Execute(listener));

            return result;
        }
    }
}