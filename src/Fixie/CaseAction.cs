namespace Fixie
{
    /// <summary>
    /// An action to perform for a single test case. This action may perform
    /// other work before, after, or instead of executing the test case.
    /// </summary>
    public delegate void CaseAction(Case @case); //Not long for this world.
}