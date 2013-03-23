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
                    .Where(type => type.IsClass && !type.IsAbstract && ClassIsFixture(type))
                    .Select(fixtureClass => new ClassFixture(fixtureClass));
            }
        }

        bool ClassIsFixture(Type concreteClass)
        {
            return concreteClass.Name.EndsWith("Tests") && concreteClass.GetConstructor(Type.EmptyTypes) != null;
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