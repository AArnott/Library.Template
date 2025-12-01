// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncWaitQueueUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Coordination;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Coordination
{
    /// <summary>
    /// Class CoreAsyncWaitQueueUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncWaitQueueUnitTests))]

    public class CoreAsyncWaitQueueUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncWaitQueueUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncWaitQueueUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void IsEmpty_WhenEmpty_IsTrue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void IsEmpty_WithOneItem_IsFalse()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            queue.Enqueue();
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void IsEmpty_WithTwoItems_IsFalse()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            queue.Enqueue();
            queue.Enqueue();
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void Dequeue_SynchronouslyCompletesTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            Task<object>? task = queue.Enqueue();
            queue.Dequeue();
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task Dequeue_WithTwoItems_OnlyCompletesFirstItem()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            Task<object>? task1 = queue.Enqueue();
            Task<object>? task2 = queue.Enqueue();
            queue.Dequeue();
            Assert.True(task1.IsCompleted);
            await CoreAsyncAssert.NeverCompletesAsync(task2);
        }

        [Fact]
        public async Task Dequeue_WithResult_SynchronouslyCompletesWithResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            object? result = new();
            Task<object>? task = queue.Enqueue();
            queue.Dequeue(result);
            Assert.Same(result, await task);
        }

        [Fact]
        public async Task Dequeue_WithoutResult_SynchronouslyCompletesWithDefaultResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            Task<object>? task = queue.Enqueue();
            queue.Dequeue();
            Assert.Equal(default, await task);
        }

        [Fact]
        public void DequeueAll_SynchronouslyCompletesAllTasks()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            Task<object>? task1 = queue.Enqueue();
            Task<object>? task2 = queue.Enqueue();
            queue.DequeueAll();
            Assert.True(task1.IsCompleted);
            Assert.True(task2.IsCompleted);
        }

        [Fact]
        public async Task DequeueAll_WithoutResult_SynchronouslyCompletesAllTasksWithDefaultResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            Task<object>? task1 = queue.Enqueue();
            Task<object>? task2 = queue.Enqueue();
            queue.DequeueAll();
            Assert.Equal(default, await task1);
            Assert.Equal(default, await task2);
        }

        [Fact]
        public async Task DequeueAll_WithResult_CompletesAllTasksWithResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            object? result = new();
            Task<object>? task1 = queue.Enqueue();
            Task<object>? task2 = queue.Enqueue();
            queue.DequeueAll(result);
            Assert.Same(result, await task1);
            Assert.Same(result, await task2);
        }

        [Fact]
        public void TryCancel_EntryFound_SynchronouslyCancelsTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            Task<object>? task = queue.Enqueue();
            queue.TryCancel(task, new CancellationToken(true));
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void TryCancel_EntryFound_RemovesTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            Task<object>? task = queue.Enqueue();
            queue.TryCancel(task, new CancellationToken(true));
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void TryCancel_EntryNotFound_DoesNotRemoveTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            Task<object>? task = queue.Enqueue();
            queue.Enqueue();
            queue.Dequeue();
            queue.TryCancel(task, new CancellationToken(true));
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public async Task Cancelled_WhenInQueue_CancelsTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            using var cts = new CancellationTokenSource();
            Task<object>? task = queue.Enqueue(new object(), cts.Token);
            cts.Cancel();
            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(task);
        }

        [Fact]
        public async Task Cancelled_WhenInQueue_RemovesTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            using var cts = new CancellationTokenSource();
            Task<object>? task = queue.Enqueue(new object(), cts.Token);
            cts.Cancel();
            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void Cancelled_WhenNotInQueue_DoesNotRemoveTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            using var cts = new CancellationTokenSource();
            Task<object>? task = queue.Enqueue(new object(), cts.Token);
            Task<object>? t_ = queue.Enqueue();
            queue.Dequeue();
            cts.Cancel();
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void Cancelled_BeforeEnqueue_SynchronouslyCancelsTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            Task<object>? task = queue.Enqueue(new object(), cts.Token);
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void Cancelled_BeforeEnqueue_RemovesTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as ICoreAsyncWaitQueue<object>;
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            Task<object>? task = queue.Enqueue(new object(), cts.Token);
            Assert.True(queue.IsEmpty);
        }
    }
}
