// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncAutoResetEventUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Coordination;
using NetworkVisor.Core.Async.Tasks.Synchronous;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Coordination
{
    /// <summary>
    /// Class CoreAsyncAutoResetEventUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncAutoResetEventUnitTests))]

    public class CoreAsyncAutoResetEventUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncAutoResetEventUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncAutoResetEventUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task CoreWaitAsync_Unset_IsNotCompleted()
        {
            var are = new CoreAsyncAutoResetEvent();

            Task? task = are.WaitAsync();

            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public void CoreWaitAsync_AfterSet_CompletesSynchronously()
        {
            var are = new CoreAsyncAutoResetEvent();

            are.Set();
            Task? task = are.WaitAsync();

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void CoreWaitAsync_Set_CompletesSynchronously()
        {
            var are = new CoreAsyncAutoResetEvent(true);

            Task? task = are.WaitAsync();

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task MultipleCoreWaitAsync_AfterSet_OnlyOneIsCompleted()
        {
            var are = new CoreAsyncAutoResetEvent();

            are.Set();
            Task? task1 = are.WaitAsync();
            Task? task2 = are.WaitAsync();

            Assert.True(task1.IsCompleted);
            await CoreAsyncAssert.NeverCompletesAsync(task2);
        }

        [Fact]
        public async Task MultipleCoreWaitAsync_Set_OnlyOneIsCompleted()
        {
            var are = new CoreAsyncAutoResetEvent(true);

            Task? task1 = are.WaitAsync();
            Task? task2 = are.WaitAsync();

            Assert.True(task1.IsCompleted);
            await CoreAsyncAssert.NeverCompletesAsync(task2);
        }

        [Fact]
        public async Task MultipleCoreWaitAsync_AfterMultipleSet_OnlyOneIsCompleted()
        {
            var are = new CoreAsyncAutoResetEvent();

            are.Set();
            are.Set();
            Task? task1 = are.WaitAsync();
            Task? task2 = are.WaitAsync();

            Assert.True(task1.IsCompleted);
            await CoreAsyncAssert.NeverCompletesAsync(task2);
        }

        [Fact]
        public void CoreWaitAsync_PreCancelled_Set_SynchronouslyCompletesWait()
        {
            var are = new CoreAsyncAutoResetEvent(true);
            var token = new CancellationToken(true);

            Task? task = are.WaitAsync(token);

            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public async Task CoreWaitAsync_Cancelled_DoesNotAutoReset()
        {
            var are = new CoreAsyncAutoResetEvent();
            using var cts = new CancellationTokenSource();

            cts.Cancel();
            Task? task1 = are.WaitAsync(cts.Token);
            task1.WaitWithoutException();
            are.Set();
            Task? task2 = are.WaitAsync();

            await task2;
        }

        [Fact]
        public void CoreWaitAsync_PreCancelled_Unset_SynchronouslyCancels()
        {
            var are = new CoreAsyncAutoResetEvent(false);
            var token = new CancellationToken(true);

            Task? task = are.WaitAsync(token);

            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

#if TODO
        [Fact]
        public void WaitAsyncFromCustomSynchronizationContext_PreCancelled_Unset_SynchronouslyCancels()
        {
            CoreAsyncContext.Run(() =>
            {
                var are = new CoreAsyncAutoResetEvent(false);
                var token = new CancellationToken(true);

                var task = are.WaitAsync(token);

                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsCanceled);
                Assert.IsFalse(task.IsFaulted);
            });
        }
#endif

        [Fact]
        public async Task CoreWaitAsync_Cancelled_ThrowsException()
        {
            var are = new CoreAsyncAutoResetEvent();
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            Task? task = are.WaitAsync(cts.Token);
            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(task);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var are = new CoreAsyncAutoResetEvent();
            Assert.NotEqual(0, are.Id);
        }
    }
}
