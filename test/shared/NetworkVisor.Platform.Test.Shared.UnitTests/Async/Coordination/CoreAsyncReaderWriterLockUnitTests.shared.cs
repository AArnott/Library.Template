// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncReaderWriterLockUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Async.Coordination;
using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.Async.Tasks.Interop;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Coordination
{
    /// <summary>
    /// Class CoreAsyncReaderWriterLockUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncReaderWriterLockUnitTests))]

    public class CoreAsyncReaderWriterLockUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncReaderWriterLockUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncReaderWriterLockUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task Unlocked_PermitsWriterLock()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            await rwl.WriterLockAsync();
        }

        [Fact]
        public async Task Unlocked_PermitsMultipleReaderLocks()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            await rwl.ReaderLockAsync();
            await rwl.ReaderLockAsync();
        }

        [Fact]
        public async Task WriteLocked_PreventsAnotherWriterLock()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            await rwl.WriterLockAsync();
            Task<IDisposable> task = rwl.WriterLockAsync().AsTask();
            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task WriteLocked_PreventsReaderLock()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            await rwl.WriterLockAsync();
            Task<IDisposable> task = rwl.ReaderLockAsync().AsTask();
            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task WriteLocked_Unlocked_PermitsAnotherWriterLock()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            TaskCompletionSource<object> firstWriteLockTaken = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            TaskCompletionSource<object> releaseFirstWriteLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            var task = Task.Run(async () =>
            {
                using (await rwl.WriterLockAsync())
                {
                    firstWriteLockTaken.SetResult(null!);
                    await releaseFirstWriteLock.Task;
                }
            });
            await firstWriteLockTaken.Task;
            Task<IDisposable> lockTask = rwl.WriterLockAsync().AsTask();
            Assert.False(lockTask.IsCompleted);
            releaseFirstWriteLock.SetResult(null!);
            await lockTask;
        }

        [Fact]
        public async Task ReadLocked_PreventsWriterLock()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            await rwl.ReaderLockAsync();
            Task<IDisposable> task = rwl.WriterLockAsync().AsTask();
            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            Assert.NotEqual(0, rwl.Id);
        }

        [Fact]
        public void WriterLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            var token = new CancellationToken(true);

            Task<IDisposable> task = rwl.WriterLockAsync(token).AsTask();

            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public void WriterLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            var token = new CancellationToken(true);
            rwl.WriterLockAsync();

            Task<IDisposable> task = rwl.WriterLockAsync(token).AsTask();

            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public void ReaderLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            var token = new CancellationToken(true);

            Task<IDisposable> task = rwl.ReaderLockAsync(token).AsTask();

            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public void ReaderLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            var token = new CancellationToken(true);
            rwl.WriterLockAsync();

            Task<IDisposable> task = rwl.ReaderLockAsync(token).AsTask();

            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public async Task WriteLocked_WriterLockCancelled_DoesNotTakeLockWhenUnlocked()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            using (await rwl.WriterLockAsync())
            {
                using var cts = new CancellationTokenSource();
                Task<IDisposable> task = rwl.WriterLockAsync(cts.Token).AsTask();
                cts.Cancel();
                await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            }

            await rwl.WriterLockAsync();
        }

        [Fact]
        public async Task WriteLocked_ReaderLockCancelled_DoesNotTakeLockWhenUnlocked()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            using (await rwl.WriterLockAsync())
            {
                var cts = new CancellationTokenSource();
                Task<IDisposable> task = rwl.ReaderLockAsync(cts.Token).AsTask();
                cts.Cancel();
                await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            }

            await rwl.ReaderLockAsync();
        }

        [Fact]
        public async Task LockReleased_WriteTakesPriorityOverRead()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            Task writeLock, readLock;
            using (await rwl.WriterLockAsync())
            {
                readLock = rwl.ReaderLockAsync().AsTask();
                writeLock = rwl.WriterLockAsync().AsTask();
            }

            await writeLock;
            await CoreAsyncAssert.NeverCompletesAsync(readLock);
        }

        [Fact]
        public async Task ReaderLocked_ReaderReleased_ReaderAndWriterWaiting_DoesNotReleaseReaderOrWriter()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            Task readLock, writeLock;
            await rwl.ReaderLockAsync();
            using (await rwl.ReaderLockAsync())
            {
                writeLock = rwl.WriterLockAsync().AsTask();
                readLock = rwl.ReaderLockAsync().AsTask();
            }

            await Task.WhenAll(
                CoreAsyncAssert.NeverCompletesAsync(writeLock),
                CoreAsyncAssert.NeverCompletesAsync(readLock));
        }

        [Fact]
        public async Task LoadTest()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            var readKeys = new List<IDisposable>();
            for (int i = 0; i != 1000; ++i)
            {
                readKeys.Add(rwl.ReaderLock());
            }

            var writeTask = Task.Run(() => { rwl.WriterLock().Dispose(); });
            var readTasks = new List<Task>();
            for (int i = 0; i != 100; ++i)
            {
                readTasks.Add(Task.Run(() => rwl.ReaderLock().Dispose()));
            }

            this.TestDelay(1000, this.TestCaseLogger).Should().BeTrue();

            foreach (IDisposable? readKey in readKeys)
            {
                readKey.Dispose();
            }

            await writeTask;
            foreach (Task? readTask in readTasks)
            {
                await readTask;
            }
        }

        [Fact]
        public async Task ReadLock_WriteLockCanceled_TakesLock()
        {
            var rwl = new CoreAsyncReaderWriterLock();
            IDisposable readKey = rwl.ReaderLock();
            using var cts = new CancellationTokenSource();

            var writerLockReady = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var writerLockTask = Task.Run(async () =>
            {
                AwaitableDisposable<IDisposable> writeKeyTask = rwl.WriterLockAsync(cts.Token);
                writerLockReady.SetResult(null!);
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writeKeyTask);
            });
            await writerLockReady.Task;

            var readerLockReady = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var readerLockTask = Task.Run(async () =>
            {
                AwaitableDisposable<IDisposable> readKeyTask = rwl.ReaderLockAsync();
                readerLockReady.SetResult(null!);
                await readKeyTask;
            });

            await readerLockReady.Task;
            cts.Cancel();

            await readerLockTask;
        }
    }
}
