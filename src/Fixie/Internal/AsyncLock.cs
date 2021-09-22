namespace Fixie.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class AsyncLock
    {
        readonly SemaphoreSlim semaphore;

        public AsyncLock()
        {
            semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<DisposableLock> LockAsync(CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken);
            return new DisposableLock(semaphore);
        }

        public class DisposableLock : IDisposable
        {
            readonly SemaphoreSlim semaphore;

            public DisposableLock(SemaphoreSlim semaphore)
            {
                this.semaphore = semaphore;
            }

            public void Dispose()
            {
                semaphore.Release();
            }
        }
    }
}
