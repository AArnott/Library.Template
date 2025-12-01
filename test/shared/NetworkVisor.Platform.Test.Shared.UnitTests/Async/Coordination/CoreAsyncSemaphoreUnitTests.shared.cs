// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncSemaphoreUnitTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Coordination
{
    /// <summary>
    /// Class CoreAsyncSemaphoreUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncSemaphoreUnitTests))]

    public class CoreAsyncSemaphoreUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncSemaphoreUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncSemaphoreUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task WaitAsync_NoSlotsAvailable_IsNotCompleted()
        {
            var semaphore = new CoreAsyncSemaphore(0);
            Assert.Equal(0, semaphore.CurrentCount);
            Task? task = semaphore.WaitAsync();
            Assert.Equal(0, semaphore.CurrentCount);
            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task WaitAsync_SlotAvailable_IsCompleted()
        {
            var semaphore = new CoreAsyncSemaphore(1);
            Assert.Equal(1, semaphore.CurrentCount);
            Task? task1 = semaphore.WaitAsync();
            Assert.Equal(0, semaphore.CurrentCount);
            Assert.True(task1.IsCompleted);
            Task? task2 = semaphore.WaitAsync();
            Assert.Equal(0, semaphore.CurrentCount);
            await CoreAsyncAssert.NeverCompletesAsync(task2);
        }

        [Fact]
        public void WaitAsync_PreCancelled_SlotAvailable_SucceedsSynchronously()
        {
            var semaphore = new CoreAsyncSemaphore(1);
            Assert.Equal(1, semaphore.CurrentCount);
            var token = new CancellationToken(true);

            Task? task = semaphore.WaitAsync(token);

            Assert.Equal(0, semaphore.CurrentCount);
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public void WaitAsync_PreCancelled_NoSlotAvailable_CancelsSynchronously()
        {
            var semaphore = new CoreAsyncSemaphore(0);
            Assert.Equal(0, semaphore.CurrentCount);
            var token = new CancellationToken(true);

            Task? task = semaphore.WaitAsync(token);

            Assert.Equal(0, semaphore.CurrentCount);
            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public async Task WaitAsync_Cancelled_DoesNotTakeSlot()
        {
            var semaphore = new CoreAsyncSemaphore(0);
            Assert.Equal(0, semaphore.CurrentCount);
            using var cts = new CancellationTokenSource();
            Task? task = semaphore.WaitAsync(cts.Token);
            Assert.Equal(0, semaphore.CurrentCount);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            Func<Task> fx = () => task;
            await fx.Should().ThrowAsync<OperationCanceledException>();

            semaphore.Release();
            Assert.Equal(1, semaphore.CurrentCount);
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void Release_WithoutWaiters_IncrementsCount()
        {
            var semaphore = new CoreAsyncSemaphore(0);
            Assert.Equal(0, semaphore.CurrentCount);
            semaphore.Release();
            Assert.Equal(1, semaphore.CurrentCount);
            Task? task = semaphore.WaitAsync();
            Assert.Equal(0, semaphore.CurrentCount);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task Release_WithWaiters_ReleasesWaiters()
        {
            var semaphore = new CoreAsyncSemaphore(0);
            Assert.Equal(0, semaphore.CurrentCount);
            Task? task = semaphore.WaitAsync();
            Assert.Equal(0, semaphore.CurrentCount);
            Assert.False(task.IsCompleted);
            semaphore.Release();
            Assert.Equal(0, semaphore.CurrentCount);
            await task;
        }

        [Fact]
        public void Release_Overflow_ThrowsException()
        {
            var semaphore = new CoreAsyncSemaphore(long.MaxValue);
            Assert.Equal(long.MaxValue, semaphore.CurrentCount);
            CoreAsyncAssert.Throws<OverflowException>(semaphore.Release);
        }

        [Fact]
        public void Release_ZeroSlots_HasNoEffect()
        {
            var semaphore = new CoreAsyncSemaphore(1);
            Assert.Equal(1, semaphore.CurrentCount);
            semaphore.Release(0);
            Assert.Equal(1, semaphore.CurrentCount);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var semaphore = new CoreAsyncSemaphore(0);
            Assert.NotEqual(0, semaphore.Id);
        }
    }
}
