// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// // ***********************************************************************
// <copyright file="CoreAsyncTaskExtensionsUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>

using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Tasks
{
    /// <summary>
    /// Class CoreAsyncTaskExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncTaskExtensionsUnitTests))]

    public class CoreAsyncTaskExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncTaskExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncTaskExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void WaitAsyncTResult_TokenThatCannotCancel_ReturnsSourceTask()
        {
            var tcs = new TaskCompletionSource<object>();
            Task<object>? task = tcs.Task.CoreWaitAsync(CancellationToken.None, this.TestCaseLogger);

            Assert.Same(tcs.Task, task);
        }

        [Fact]
        public void WaitAsyncTResult_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            Task<object>? task = tcs.Task.CoreWaitAsync(token, this.TestCaseLogger);

            Assert.True(task.IsCanceled);
            Assert.Equal(token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public async Task WaitAsyncTResult_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            using var cts = new CancellationTokenSource();
            Task<object>? task = tcs.Task.CoreWaitAsync(cts.Token, this.TestCaseLogger);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public void WaitAsync_TokenThatCannotCancel_ReturnsSourceTask()
        {
            var tcs = new TaskCompletionSource<object>();
            Task? task = ((Task)tcs.Task).CoreWaitAsync(CancellationToken.None, this.TestCaseLogger);

            Assert.Same(tcs.Task, task);
        }

        [Fact]
        public void WaitAsync_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            Task? task = ((Task)tcs.Task).CoreWaitAsync(token, this.TestCaseLogger);

            Assert.True(task.IsCanceled);
            Assert.Equal(token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public async Task WaitAsync_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            using var cts = new CancellationTokenSource();
            Task? task = ((Task)tcs.Task).CoreWaitAsync(cts.Token, this.TestCaseLogger);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public void WhenAnyTResult_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            Task<Task<object>>? task = new[] { tcs.Task }.WhenAnyWaitAsync(token, this.TestCaseLogger);

            Assert.True(task.IsCanceled);
            Assert.Equal(token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public async Task WhenAnyTResult_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            using var cts = new CancellationTokenSource();
            Task<Task<object>>? task = new[] { tcs.Task }.WhenAnyWaitAsync(cts.Token, this.TestCaseLogger);
            Assert.False(task.IsCompleted);

            tcs.SetResult(null!);

            Task<object>? result = await task;
            Assert.Same(tcs.Task, result);
        }

        [Fact]
        public async Task WhenAnyTResult_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            using var cts = new CancellationTokenSource();
            Task<Task<object>>? task = new[] { tcs.Task }.WhenAnyWaitAsync(cts.Token, this.TestCaseLogger);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public void WhenAny_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            Task<Task>? task = new Task[] { tcs.Task }.WhenAnyWaitAsync(token, this.TestCaseLogger);

            Assert.True(task.IsCanceled);
            Assert.Equal(token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public async Task WhenAny_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            using var cts = new CancellationTokenSource();
            Task<Task>? task = new Task[] { tcs.Task }.WhenAnyWaitAsync(cts.Token, this.TestCaseLogger);
            Assert.False(task.IsCompleted);

            tcs.SetResult(null!);

            Task? result = await task;
            Assert.Same(tcs.Task, result);
        }

        [Fact]
        public async Task WhenAny_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            using var cts = new CancellationTokenSource();
            Task<Task>? task = new Task[] { tcs.Task }.WhenAnyWaitAsync(cts.Token, this.TestCaseLogger);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public async Task WhenAnyTResultWithoutToken_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            Task<Task<object>>? task = new[] { tcs.Task }.WhenAny();
            Assert.False(task.IsCompleted);

            tcs.SetResult(null!);

            Task<object>? result = await task;
            Assert.Same(tcs.Task, result);
        }

        [Fact]
        public async Task WhenAnyWithoutToken_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            Task<Task>? task = new Task[] { tcs.Task }.WhenAny();
            Assert.False(task.IsCompleted);

            tcs.SetResult(null!);

            Task? result = await task;
            Assert.Same(tcs.Task, result);
        }

        [Fact]
        public async Task WhenAllTResult_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            Task<object[]>? task = new[] { tcs.Task }.WhenAll();
            Assert.False(task.IsCompleted);

            object? expectedResult = new();
            tcs.SetResult(expectedResult);

            object[]? result = await task;
            Assert.Equal(new[] { expectedResult }, result);
        }

        [Fact]
        public async Task WhenAll_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            Task? task = new Task[] { tcs.Task }.WhenAll();
            Assert.False(task.IsCompleted);

            object? expectedResult = new();
            tcs.SetResult(expectedResult);

            await task;
        }

        [Fact]
        public async Task OrderByCompletion_OrdersByCompletion()
        {
            var tcs = new TaskCompletionSource<int>[] { new(), new() };
            List<Task<int>>? results = tcs.Select(x => x.Task).OrderByCompletion();

            Assert.False(results[0].IsCompleted);
            Assert.False(results[1].IsCompleted);

            tcs[1].SetResult(13);
            int result0 = await results[0];
            Assert.False(results[1].IsCompleted);
            Assert.Equal(13, result0);

            tcs[0].SetResult(17);
            int result1 = await results[1];
            Assert.Equal(13, result0);
            Assert.Equal(17, result1);
        }

        [Fact]
        public async Task OrderByCompletion_PropagatesFaultOnFirstCompletion()
        {
            var tcs = new TaskCompletionSource<int>[] { new(), new() };
            List<Task<int>>? results = tcs.Select(x => x.Task).OrderByCompletion();

            tcs[1].SetException(new InvalidOperationException("test message"));
            try
            {
                await results[0];
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equal("test message", ex.Message);
                return;
            }

            Assert.True(false);
        }

        [Fact]
        public async Task OrderByCompletion_PropagatesFaultOnSecondCompletion()
        {
            var tcs = new TaskCompletionSource<int>[] { new(), new() };
            List<Task<int>>? results = tcs.Select(x => x.Task).OrderByCompletion();

            tcs[0].SetResult(13);
            tcs[1].SetException(new InvalidOperationException("test message"));
            await results[0];
            try
            {
                await results[1];
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equal("test message", ex.Message);
                return;
            }

            Assert.True(false);
        }

        [Fact]
        public async Task OrderByCompletion_PropagatesCancelOnFirstCompletion()
        {
            var tcs = new TaskCompletionSource<int>[] { new(), new() };
            List<Task<int>>? results = tcs.Select(x => x.Task).OrderByCompletion();

            tcs[1].SetCanceled();
            try
            {
                await results[0];
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Assert.True(false);
        }

        [Fact]
        public async Task OrderByCompletion_PropagatesCancelOnSecondCompletion()
        {
            var tcs = new TaskCompletionSource<int>[] { new(), new() };
            List<Task<int>>? results = tcs.Select(x => x.Task).OrderByCompletion();

            tcs[0].SetResult(13);
            tcs[1].SetCanceled();
            await results[0];
            try
            {
                await results[1];
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Assert.True(false);
        }

        private static CancellationToken GetCancellationTokenFromTask(Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is OperationCanceledException oce)
                {
                    return oce.CancellationToken;
                }
            }

            return CancellationToken.None;
        }
    }
}
