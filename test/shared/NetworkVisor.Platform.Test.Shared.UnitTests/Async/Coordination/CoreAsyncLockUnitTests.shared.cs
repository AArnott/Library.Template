// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncLockUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Coordination;
using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.Async.Tasks.Interop;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Coordination
{
    /// <summary>
    /// Class CoreAsyncLockUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncLockUnitTests))]

    public class CoreAsyncLockUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncLockUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncLockUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreAsyncLock_Unlocked_SynchronouslyPermitsLock()
        {
            var mutex = new CoreAsyncLock();

            Task<IDisposable> lockTask = mutex.LockAsync().AsTask();

            Assert.True(lockTask.IsCompleted);
            Assert.False(lockTask.IsFaulted);
            Assert.False(lockTask.IsCanceled);
        }

        [Fact]
        public async Task CoreAsyncLock_Locked_PreventsLockUntilUnlocked()
        {
            var mutex = new CoreAsyncLock();
            TaskCompletionSource<object> task1HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            TaskCompletionSource<object> task1Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

            var task1 = Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    task1HasLock.SetResult(null!);
                    await task1Continue.Task;
                }
            });
            await task1HasLock.Task;

            var task2 = Task.Run(async () =>
            {
                await mutex.LockAsync();
            });

            Assert.False(task2.IsCompleted);
            task1Continue.SetResult(null!);
            await task2;
        }

        [Fact]
        public async Task CoreAsyncLock_DoubleDispose_OnlyPermitsOneTask()
        {
            var mutex = new CoreAsyncLock();
            TaskCompletionSource<object> task1HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            TaskCompletionSource<object> task1Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

            await Task.Run(async () =>
            {
                IDisposable key = await mutex.LockAsync();
                key.Dispose();
                key.Dispose();
            });

            var task1 = Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    task1HasLock.SetResult(null!);
                    await task1Continue.Task;
                }
            });
            await task1HasLock.Task;

            var task2 = Task.Run(async () =>
            {
                await mutex.LockAsync();
            });

            Assert.False(task2.IsCompleted);
            task1Continue.SetResult(null!);
            await task2;
        }

        [Fact]
        public async Task CoreAsyncLock_Locked_OnlyPermitsOneLockerAtATime()
        {
            var mutex = new CoreAsyncLock();
            TaskCompletionSource<object> task1HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            TaskCompletionSource<object> task1Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            TaskCompletionSource<object> task2Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            TaskCompletionSource<object> task2HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            TaskCompletionSource<object> task2Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

            var task1 = Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    task1HasLock.SetResult(null!);
                    await task1Continue.Task;
                }
            });
            await task1HasLock.Task;

            var task2 = Task.Run(async () =>
            {
                AwaitableDisposable<IDisposable> key = mutex.LockAsync();
                task2Ready.SetResult(null!);
                using (await key)
                {
                    task2HasLock.SetResult(null!);
                    await task2Continue.Task;
                }
            });
            await task2Ready.Task;

            var task3 = Task.Run(async () =>
            {
                await mutex.LockAsync();
            });

            task1Continue.SetResult(null!);
            await task2HasLock.Task;

            Assert.False(task3.IsCompleted);
            task2Continue.SetResult(null!);
            await task2;
            await task3;
        }

        [Fact]
        public void CoreAsyncLock_PreCancelled_Unlocked_SynchronouslyTakesLock()
        {
            var mutex = new CoreAsyncLock();
            var token = new CancellationToken(true);

            Task<IDisposable> task = mutex.LockAsync(token).AsTask();

            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public void CoreAsyncLock_PreCancelled_Locked_SynchronouslyCancels()
        {
            var mutex = new CoreAsyncLock();
            AwaitableDisposable<IDisposable> lockTask = mutex.LockAsync();
            var token = new CancellationToken(true);

            Task<IDisposable> task = mutex.LockAsync(token).AsTask();

            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public async Task CoreAsyncLock_CancelledLock_LeavesLockUnlocked()
        {
            var mutex = new CoreAsyncLock();
            using var cts = new CancellationTokenSource();
            TaskCompletionSource<object> taskReady = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

            IDisposable unlock = await mutex.LockAsync();
            var task = Task.Run(async () =>
            {
                AwaitableDisposable<IDisposable> lockTask = mutex.LockAsync(cts.Token);
                taskReady.SetResult(null!);
                await lockTask;
            });
            await taskReady.Task;
            cts.Cancel();
            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.True(task.IsCanceled);
            unlock.Dispose();

            AwaitableDisposable<IDisposable> finalLockTask = mutex.LockAsync();
            await finalLockTask;
        }

        [Fact]
        public async Task CoreAsyncLock_CanceledLock_ThrowsException()
        {
            var mutex = new CoreAsyncLock();
            using var cts = new CancellationTokenSource();

            await mutex.LockAsync();
            Task<IDisposable> canceledLockTask = mutex.LockAsync(cts.Token).AsTask();
            cts.Cancel();

            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(canceledLockTask);
        }

        [Fact]
        public async Task CoreAsyncLock_CanceledTooLate_StillTakesLock()
        {
            var mutex = new CoreAsyncLock();
            using var cts = new CancellationTokenSource();

            AwaitableDisposable<IDisposable> cancelableLockTask;
            using (await mutex.LockAsync())
            {
                cancelableLockTask = mutex.LockAsync(cts.Token);
            }

            IDisposable key = await cancelableLockTask;
            cts.Cancel();

            Task<IDisposable> nextLocker = mutex.LockAsync().AsTask();
            Assert.False(nextLocker.IsCompleted);

            key.Dispose();
            await nextLocker;
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var mutex = new CoreAsyncLock();
            Assert.NotEqual(0, mutex.Id);
        }

        [Fact(Skip = "Deadlocks")]
        public async Task CoreAsyncLock_SupportsMultipleAsynchronousLocks()
        {
            // This test deadlocks with the old CoreAsyncEx: https://github.com/StephenCleary/AsyncEx/issues/57
            await Task.Run(() =>
            {
                var asyncLock = new CoreAsyncLock();
                using var cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationTokenSource.Token;
                var task1 = Task.Run(
                    async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            using (await asyncLock.LockAsync())
                            {
                                Thread.Sleep(10);
                            }
                        }
                    });
                var task2 = Task.Run(
                    () =>
                    {
                        using (asyncLock.Lock())
                        {
                            Thread.Sleep(1000);
                        }
                    });

#pragma warning disable xUnit1031
                task2.Wait();
                cancellationTokenSource.Cancel();
                task1.Wait();
            });
        }
#pragma warning restore xUnit1031
    }
}
