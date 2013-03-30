using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie
{
    public class Suite
    {
        readonly Type[] candidateTypes;
        readonly Convention convention;

        public Suite(Convention convention, params Type[] candidateTypes)
        {
            this.candidateTypes = candidateTypes;
            this.convention = convention;
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