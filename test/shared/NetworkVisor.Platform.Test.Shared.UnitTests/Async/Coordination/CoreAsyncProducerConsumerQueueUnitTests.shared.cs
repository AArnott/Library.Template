// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncProducerConsumerQueueUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreAsyncProducerConsumerQueueUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncProducerConsumerQueueUnitTests))]

    public class CoreAsyncProducerConsumerQueueUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncProducerConsumerQueueUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncProducerConsumerQueueUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void ConstructorWithZeroMaxCount_Throws()
        {
            CoreAsyncAssert.Throws<ArgumentOutOfRangeException>(() => new CoreAsyncProducerConsumerQueue<int>(0));
        }

        [Fact]
        public void ConstructorWithZeroMaxCountAndCollection_Throws()
        {
            CoreAsyncAssert.Throws<ArgumentOutOfRangeException>(() => new CoreAsyncProducerConsumerQueue<int>(new int[0], 0));
        }

        [Fact]
        public void ConstructorWithMaxCountSmallerThanCollectionCount_Throws()
        {
            CoreAsyncAssert.Throws<ArgumentException>(() => new CoreAsyncProducerConsumerQueue<int>(new[] { 3, 5 }, 1));
        }

        [Fact]
        public async Task ConstructorWithCollection_AddsItems()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>(new[] { 3, 5, 7 });

            int result1 = await queue.DequeueAsync();
            int result2 = await queue.DequeueAsync();
            int result3 = await queue.DequeueAsync();

            Assert.Equal(3, result1);
            Assert.Equal(5, result2);
            Assert.Equal(7, result3);
        }

        [Fact]
        public async Task EnqueueAsync_SpaceAvailable_EnqueuesItem()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();

            await queue.EnqueueAsync(3);
            int result = await queue.DequeueAsync();

            Assert.Equal(3, result);
        }

        [Fact]
        public async Task EnqueueAsync_CompleteAdding_ThrowsException()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();
            queue.CompleteAdding();

            await CoreAsyncAssert.ThrowsAsync<InvalidOperationException>(() => queue.EnqueueAsync(3));
        }

        [Fact]
        public async Task DequeueAsync_EmptyAndComplete_ThrowsException()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();
            queue.CompleteAdding();

            await CoreAsyncAssert.ThrowsAsync<InvalidOperationException>(queue.DequeueAsync);
        }

        [Fact]
        public async Task DequeueAsync_Empty_DoesNotComplete()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();

            Task<int>? task = queue.DequeueAsync();

            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task DequeueAsync_Empty_ItemAdded_Completes()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();
            Task<int>? task = queue.DequeueAsync();

            await queue.EnqueueAsync(13);
            int result = await task;

            Assert.Equal(13, result);
        }

        [Fact]
        public async Task DequeueAsync_Cancelled_Throws()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();
            using var cts = new CancellationTokenSource();
            Task<int>? task = queue.DequeueAsync(cts.Token);

            cts.Cancel();

            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(() => task);
        }

        [Fact]
        public async Task EnqueueAsync_Full_DoesNotComplete()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>(new[] { 13 }, 1);

            Task? task = queue.EnqueueAsync(7);

            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task EnqueueAsync_SpaceAvailable_Completes()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
            Task? task = queue.EnqueueAsync(7);

            await queue.DequeueAsync();

            await task;
        }

        [Fact]
        public async Task EnqueueAsync_Cancelled_Throws()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
            using var cts = new CancellationTokenSource();
            Task? task = queue.EnqueueAsync(7, cts.Token);

            cts.Cancel();

            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(() => task);
        }

        [Fact]
        public void CompleteAdding_MultipleTimes_DoesNotThrow()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();
            queue.CompleteAdding();

            queue.CompleteAdding();
        }

        [Fact]
        public async Task OutputAvailableAsync_NoItemsInQueue_IsNotCompleted()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();

            Task<bool>? task = queue.OutputAvailableAsync();

            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task OutputAvailableAsync_ItemInQueue_ReturnsTrue()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();
            queue.Enqueue(13);

            bool result = await queue.OutputAvailableAsync();
            Assert.True(result);
        }

        [Fact]
        public async Task OutputAvailableAsync_NoItemsAndCompleted_ReturnsFalse()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();
            queue.CompleteAdding();

            bool result = await queue.OutputAvailableAsync();
            Assert.False(result);
        }

        [Fact]
        public async Task OutputAvailableAsync_ItemInQueueAndCompleted_ReturnsTrue()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();
            queue.Enqueue(13);
            queue.CompleteAdding();

            bool result = await queue.OutputAvailableAsync();
            Assert.True(result);
        }

        [Fact]
        public async Task StandardAsyncSingleConsumerCode()
        {
            var queue = new CoreAsyncProducerConsumerQueue<int>();
            var producer = Task.Run(() =>
            {
                queue.Enqueue(3);
                queue.Enqueue(13);
                queue.Enqueue(17);
                queue.CompleteAdding();
            });

            var results = new List<int>();
            while (await queue.OutputAvailableAsync())
            {
                results.Add(queue.Dequeue());
            }

            Assert.Equal(3, results.Count);
            Assert.Equal(3, results[0]);
            Assert.Equal(13, results[1]);
            Assert.Equal(17, results[2]);
        }
    }
}
