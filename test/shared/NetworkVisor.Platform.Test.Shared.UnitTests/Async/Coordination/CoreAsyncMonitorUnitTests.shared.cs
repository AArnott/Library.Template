// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncMonitorUnitTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Coordination
{
    /// <summary>
    /// Class CoreAsyncMonitorUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncManualResetEventUnitTests))]

    public class CoreAsyncMonitorUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncMonitorUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncMonitorUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task Unlocked_PermitsLock()
        {
            var monitor = new CoreAsyncMonitor();

            AwaitableDisposable<IDisposable> task = monitor.EnterAsync();
            await task;
        }

        [Fact]
        public async Task Locked_PreventsLockUntilUnlocked()
        {
            var monitor = new CoreAsyncMonitor();
            TaskCompletionSource<object>? task1HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            TaskCompletionSource<object>? task1Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

            var task1 = Task.Run(async () =>
            {
                using (await monitor.EnterAsync())
                {
                    task1HasLock.SetResult(null!);
                    await task1Continue.Task;
                }
            });
            await task1HasLock.Task;

            Task<IDisposable>? lockTask = monitor.EnterAsync().AsTask();
            Assert.False(lockTask.IsCompleted);
            task1Continue.SetResult(null!);
            await lockTask;
        }

        [Fact]
        public async Task Pulse_ReleasesOneWaiter()
        {
            var monitor = new CoreAsyncMonitor();
            int completed = 0;
            TaskCompletionSource<object>? task1Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            TaskCompletionSource<object>? task2Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            var task1 = Task.Run(async () =>
            {
                using (await monitor.EnterAsync())
                {
                    Task? waitTask1 = monitor.WaitAsync();
                    task1Ready.SetResult(null!);
                    await waitTask1;
                    Interlocked.Increment(ref completed);
                }
            });
            await task1Ready.Task;
            var task2 = Task.Run(async () =>
            {
                using (await monitor.EnterAsync())
                {
                    Task? waitTask2 = monitor.WaitAsync();
                    task2Ready.SetResult(null!);
                    await waitTask2;
                    Interlocked.Increment(ref completed);
                }
            });
            await task2Ready.Task;

            using (await monitor.EnterAsync())
            {
                monitor.Pulse();
            }

            await Task.WhenAny(task1, task2);
            int result = Interlocked.CompareExchange(ref completed, 0, 0);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task PulseAll_ReleasesAllWaiters()
        {
            var monitor = new CoreAsyncMonitor();
            int completed = 0;
            TaskCompletionSource<object>? task1Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            TaskCompletionSource<object>? task2Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            Task? waitTask1 = null;
            var task1 = Task.Run(async () =>
            {
                using (await monitor.EnterAsync())
                {
                    waitTask1 = monitor.WaitAsync();
                    task1Ready.SetResult(null!);
                    await waitTask1;
                    Interlocked.Increment(ref completed);
                }
            });
            await task1Ready.Task;
            Task? waitTask2 = null;
            var task2 = Task.Run(async () =>
            {
                using (await monitor.EnterAsync())
                {
                    waitTask2 = monitor.WaitAsync();
                    task2Ready.SetResult(null!);
                    await waitTask2;
                    Interlocked.Increment(ref completed);
                }
            });
            await task2Ready.Task;

            AwaitableDisposable<IDisposable> lockTask3 = monitor.EnterAsync();
            using (await lockTask3)
            {
                monitor.PulseAll();
            }

            await Task.WhenAll(task1, task2);
            int result = Interlocked.CompareExchange(ref completed, 0, 0);

            Assert.Equal(2, result);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var monitor = new CoreAsyncMonitor();
            Assert.NotEqual(0, monitor.Id);
        }
    }
}
