namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System;
    using System.Threading.Tasks;

    public class SourceLocationSamples
    {
        public void Empty_OneLine() { } // Debug = Release = 8

        public void Empty_TwoLines()
        { // Debug = 11
        } // Release = 12

        public void Empty_ThreeLines()
        { // Debug = 15

        } // Release = 17

        public void Simple()
        { // Debug = 20
            int answer = 42; // Release = 21
            Console.Write(answer);
        }

        public void Generic<T>(T x)
        { // Debug = 26
            Console.WriteLine(); // Release = 27
        }

        public async void AsyncMethod_Void()
        { // Debug = 31
            int answer = 42; // Release = 32
            await Task.Delay(0);
            Console.Write(answer);
        }

        public async Task AsyncMethod_Task()
        { // Debug = 38
            int answer = 42; // Release = 39
            await Task.Delay(0);
            Console.Write(answer);
        }

        public async Task<int> AsyncMethod_TaskOfT()
        { // Debug = 45
            int answer = 42; // Release = 46
            await Task.Delay(0);
            return answer;
        }

        public class NestedClass
        {
            public void NestedMethod()
            { // Debug = 54
                int answer = 42; // Release = 55
                Console.Write(answer);
            }
        }

#line hidden
        public void Hidden()
        {
        }
#line default

        public void Overloaded() { } // Debug = Release = 66

        public void Overloaded(int y) { } // Debug = Release = 68

        public class BaseClass
        {
            public void Inherited() { } // Debug = Release = 72
        }

        public class ChildClass : BaseClass
        {
        }
    }
}
