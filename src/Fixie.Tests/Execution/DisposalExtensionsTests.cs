namespace Fixie.Tests.Execution
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Assertions;
    using Fixie.Execution;

    public class DisposalExtensionsTests
    {
        public void CanDetectWhetherTypeIsDisposable()
        {
            typeof(WrongReturnType).IsDisposable().ShouldBeFalse();
            typeof(NonDisposableWithDisposeMethod).IsDisposable().ShouldBeFalse();

            typeof(Disposable).IsDisposable().ShouldBeTrue();
            typeof(ChildOfDisposable).IsDisposable().ShouldBeTrue();
        }

        public void CanDetectWhetherMethodHasDisposeSignature()
        {
            Method<WrongReturnType>(typeof(int), "Dispose").HasDisposeSignature().ShouldBeFalse();
            Method<Disposable>(typeof(void), "NotNamedDispose").HasDisposeSignature().ShouldBeFalse();
            Method<Disposable>(typeof(void), "Dispose", typeof(bool)).HasDisposeSignature().ShouldBeFalse();

            Method<NonDisposableWithDisposeMethod>(typeof(void), "Dispose").HasDisposeSignature().ShouldBeTrue();
            Method<Disposable>(typeof(void), "Dispose").HasDisposeSignature().ShouldBeTrue();
        }

        class WrongReturnType
        {
            public int Dispose() => 0;
        }

        class NonDisposableWithDisposeMethod
        {
            public void Dispose() { }
        }

        class Disposable : NonDisposableWithDisposeMethod, IDisposable
        {
            public void Dispose(bool disposing) { }
            public void NotNamedDispose() { }
        }

        class ChildOfDisposable : Disposable { }

        private static MethodInfo Method<T>(Type returnType, string name, params Type[] parameterTypes)
            => typeof(T).GetInstanceMethods().Single(m => m.HasSignature(returnType, name, parameterTypes));
    }
}
