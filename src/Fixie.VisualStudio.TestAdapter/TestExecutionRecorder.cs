namespace Fixie.VisualStudio.TestAdapter
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    public class TestExecutionRecorder
    {
        readonly ITestExecutionRecorder recorder;

        public TestExecutionRecorder(ITestExecutionRecorder recorder)
        {
            this.recorder = recorder;
        }

        public void RecordResult(TestResult testResult)
        {
            recorder.RecordResult(testResult);
        }
    }
}