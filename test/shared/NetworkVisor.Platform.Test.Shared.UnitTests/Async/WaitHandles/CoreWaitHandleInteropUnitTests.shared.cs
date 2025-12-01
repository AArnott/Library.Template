// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreWaitHandleInteropUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Interop;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.WaitHandles
{
    /// <summary>
    /// Class CoreWaitHandleInteropUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreWaitHandleInteropUnitTests))]

    public class CoreWaitHandleInteropUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreWaitHandleInteropUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreWaitHandleInteropUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void FromWaitHandle_SignaledHandle_SynchronouslyCompletes()
        {
            var mre = new ManualResetEvent(true);
            Task task = WaitHandleAsyncFactory.FromWaitHandle(mre);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task FromWaitHandle_SignaledHandleWithZeroTimeout_SynchronouslyCompletesWithTrueResult()
        {
            var mre = new ManualResetEvent(true);
            Task<bool> task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.Zero);
            Assert.True(task.IsCompleted);
            Assert.True(await task);
        }

        [Fact]
        public async Task FromWaitHandle_UnsignaledHandleWithZeroTimeout_SynchronouslyCompletesWithFalseResult()
        {
            var mre = new ManualResetEvent(false);
            Task<bool> task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.Zero);
            Assert.True(task.IsCompleted);
            Assert.False(await task);
        }

        [Fact]
        public void FromWaitHandle_SignaledHandleWithCanceledToken_SynchronouslyCompletes()
        {
            var mre = new ManualResetEvent(true);
            Task task = WaitHandleAsyncFactory.FromWaitHandle(mre, new CancellationToken(true));
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void FromWaitHandle_UnsignaledHandleWithCanceledToken_SynchronouslyCancels()
        {
            var mre = new ManualResetEvent(false);
            Task task = WaitHandleAsyncFactory.FromWaitHandle(mre, new CancellationToken(true));
            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public async Task FromWaitHandle_SignaledHandleWithZeroTimeoutAndCanceledToken_SynchronouslyCompletesWithTrueResult()
        {
            var mre = new ManualResetEvent(true);
            Task<bool> task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.Zero, new CancellationToken(true));
            Assert.True(task.IsCompleted);
            Assert.True(await task);
        }

        [Fact]
        public async Task FromWaitHandle_UnsignaledHandleWithZeroTimeoutAndCanceledToken_SynchronouslyCompletesWithFalseResult()
        {
            var mre = new ManualResetEvent(false);
            Task<bool> task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.Zero, new CancellationToken(true));
            Assert.True(task.IsCompleted);
            Assert.False(await task);
        }

        [Fact]
        public async Task FromWaitHandle_HandleSignalled_Completes()
        {
            var mre = new ManualResetEvent(false);
            Task task = WaitHandleAsyncFactory.FromWaitHandle(mre);
            Assert.False(task.IsCompleted);
            mre.Set();
            await task;
        }

        [Fact]
        public async Task FromWaitHandle_HandleSignalledBeforeTimeout_CompletesWithTrueResult()
        {
            var mre = new ManualResetEvent(false);
            Task<bool> task = WaitHandleAsyncFactory.FromWaitHandle(mre, Timeout.InfiniteTimeSpan);
            Assert.False(task.IsCompleted);
            mre.Set();
            bool result = await task;
            Assert.True(result);
        }

        [Fact]
        public async Task FromWaitHandle_TimeoutBeforeHandleSignalled_CompletesWithFalseResult()
        {
            var mre = new ManualResetEvent(false);
            Task<bool> task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.FromMilliseconds(10));
            bool result = await task;
            Assert.False(result);
        }

        [Fact]
        public async Task FromWaitHandle_HandleSignalledBeforeCanceled_CompletesSuccessfully()
        {
            var mre = new ManualResetEvent(false);
            using var cts = new CancellationTokenSource();
            Task task = WaitHandleAsyncFactory.FromWaitHandle(mre, cts.Token);
            Assert.False(task.IsCompleted);
            mre.Set();
            await task;
        }

        [Fact]
        public async Task FromWaitHandle_CanceledBeforeHandleSignalled_CompletesCanceled()
        {
            var mre = new ManualResetEvent(false);
            using var cts = new CancellationTokenSource();
            Task task = WaitHandleAsyncFactory.FromWaitHandle(mre, cts.Token);
            Assert.False(task.IsCompleted);
            cts.Cancel();
            await CoreAsyncAssert.CancelsAsync(task);
        }

        [Fact]
        public async Task FromWaitHandle_HandleSignalledBeforeTimeoutOrCanceled_CompletesWithTrueResult()
        {
            var mre = new ManualResetEvent(false);
            using var cts = new CancellationTokenSource();
            Task<bool> task = WaitHandleAsyncFactory.FromWaitHandle(mre, Timeout.InfiniteTimeSpan, cts.Token);
            Assert.False(task.IsCompleted);
            mre.Set();
            bool result = await task;
            Assert.True(result);
        }

        [Fact]
        public async Task FromWaitHandle_TimeoutBeforeHandleSignalledOrCanceled_CompletesWithFalseResult()
        {
            var mre = new ManualResetEvent(false);
            using var cts = new CancellationTokenSource();
            Task<bool> task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.FromMilliseconds(10), cts.Token);
            bool result = await task;
            Assert.False(result);
        }

        [Fact]
        public async Task FromWaitHandle_CanceledBeforeTimeoutOrHandleSignalled_CompletesCanceled()
        {
            var mre = new ManualResetEvent(false);
            using var cts = new CancellationTokenSource();
            Task<bool> task = WaitHandleAsyncFactory.FromWaitHandle(mre, Timeout.InfiniteTimeSpan, cts.Token);
            Assert.False(task.IsCompleted);
            cts.Cancel();
            await CoreAsyncAssert.CancelsAsync(task);
        }
    }
}
