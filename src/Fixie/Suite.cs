using System;

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
            foreach (var fixtureClass in convention.FixtureClasses(candidateTypes))
            {
                var fixture = new ClassFixture(fixtureClass, convention);
                fixture.Execute(listener);
            }
        }
    }
}