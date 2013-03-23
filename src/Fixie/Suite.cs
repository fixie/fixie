using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class Suite
    {
        readonly Type[] candidateTypes;
        readonly DefaultConvention convention;

        public Suite(Assembly assembly)
            : this(assembly.GetTypes()) { }

        public Suite(params Type[] candidateTypes)
        {
            this.candidateTypes = candidateTypes;
            convention = new DefaultConvention();
        }

        public Result Execute(Listener listener)
        {
            var result = new Result();

            foreach (var fixture in Fixtures)
                result = Result.Combine(result, fixture.Execute(listener));

            return result;
        }

        private IEnumerable<Fixture> Fixtures
        {
            get
            {
                return convention.FixtureClasses(candidateTypes)
                                 .Select(fixtureClass => new ClassFixture(fixtureClass, convention));
            }
        }
    }
}