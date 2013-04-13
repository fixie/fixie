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

        public void Execute(Listener listener)
        {
            foreach (var fixture in Fixtures)
                fixture.Execute(listener);
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