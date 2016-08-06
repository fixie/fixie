namespace Fixie.VisualStudio.TestAdapter.Wrappers
{
    using Execution;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    public class TestExecutionRecorder : LongLivedMarshalByRefObject
    {
        readonly ITestExecutionRecorder recorder;

        public TestExecutionRecorder(ITestExecutionRecorder recorder)
        {
            this.recorder = recorder;
        }

        public void RecordResult(TestResultModel testResult)
            => recorder.RecordResult(testResult.ToVisualStudioType());
    }
}