namespace Fixie.Tests.Execution
{
    using System;
    using Assertions;
    using Fixie.Execution;

    public class DisposalExtensionsTests
    {
        public void CanDetectWhetherTypeIsDisposable()
        {
            typeof(WrongReturnType).IsDisposable().ShouldBeFalse();
            typeof(WrongParameterType).IsDisposable().ShouldBeFalse();
            typeof(NonDisposableWithDisposeMethod).IsDisposable().ShouldBeFalse();

            typeof(Disposable).IsDisposable().ShouldBeTrue();
            typeof(ChildOfDisposable).IsDisposable().ShouldBeTrue();
        }

        public void CanDetectWhetherMethodHasDisposeSignature()
        {
            typeof(WrongReturnType).GetInstanceMethod("Dispose").HasDisposeSignature().ShouldBeFalse();
            typeof(WrongParameterType).GetInstanceMethod("Dispose").HasDisposeSignature().ShouldBeFalse();
            typeof(Disposable).GetInstanceMethod("NotNamedDispose").HasDisposeSignature().ShouldBeFalse();

            typeof(NonDisposableWithDisposeMethod).GetInstanceMethod("Dispose").HasDisposeSignature().ShouldBeTrue();
            typeof(Disposable).GetInstanceMethod("Dispose").HasDisposeSignature().ShouldBeTrue();
            typeof(ChildOfDisposable).GetInstanceMethod("Dispose").HasDisposeSignature().ShouldBeTrue();
        }

        class WrongReturnType { public int Dispose() => 0; }

        class WrongParameterType { public void Dispose(bool disposing) { } }

        class NonDisposableWithDisposeMethod { public void Dispose() { } }

        class Disposable : NonDisposableWithDisposeMethod, IDisposable { public void NotNamedDispose() { } }

        class ChildOfDisposable : Disposable { }
    }
}
