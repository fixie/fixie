using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie
{
    public class ExecutionPlan
    {
        readonly BehaviorChain<ClassExecution> classBehaviorChain;
        readonly BehaviorChain<InstanceExecution> instanceBehaviorChain;
        readonly BehaviorChain<CaseExecution> caseBehaviorChain;

        public ExecutionPlan(Convention convention)
        {
            classBehaviorChain = convention.Config.BuildClassBehaviorChain();
            instanceBehaviorChain = convention.Config.BuildInstanceBehaviorChain();
            caseBehaviorChain = convention.Config.BuildCaseBehaviorChain();
        }

        public void Execute(ClassExecution classExecution)
        {
            classBehaviorChain.Execute(classExecution);
        }

        public void Execute(InstanceExecution instanceExecution)
        {
            instanceBehaviorChain.Execute(instanceExecution);
        }

        public void Execute(CaseExecution caseExecution)
        {
            caseBehaviorChain.Execute(caseExecution);
        }
    }
}