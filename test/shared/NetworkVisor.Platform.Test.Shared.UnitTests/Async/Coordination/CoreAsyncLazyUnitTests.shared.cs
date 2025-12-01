// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncLazyUnitTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;
#pragma warning disable CS0162

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Coordination
{
    /// <summary>
    /// Class CoreAsyncLazyUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncLazyUnitTests))]

    public class CoreAsyncLazyUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncLazyUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncLazyUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreAsyncLazy_NeverAwaited_DoesNotCallFunc()
        {
            static Task<int> Func()
            {
                throw new Exception();
                return Task.FromResult(13);
            }

            var lazy = new CoreAsyncLazy<int>(Func);
        }

        [Fact]
        public async Task CoreAsyncLazy_WithCallDirectFlag_CallsFuncDirectly()
        {
            int testThread = Thread.CurrentThread.ManagedThreadId;
            int funcThread = testThread + 1;
            Task<int> Func()
            {
                funcThread = Thread.CurrentThread.ManagedThreadId;
                return Task.FromResult(13);
            }

            var lazy = new CoreAsyncLazy<int>(Func, CoreAsyncLazyFlags.ExecuteOnCallingThread);

            await lazy;

            Assert.Equal(testThread, funcThread);
        }

        [Fact(Skip = "Xunit 3.0 Task")]
        public async Task CoreAsyncLazy_ByDefault_CallsFuncOnThreadPool()
        {
            int testThread = Thread.CurrentThread.ManagedThreadId;
            int funcThread = testThread;
            Task<int> Func()
            {
                funcThread = Thread.CurrentThread.ManagedThreadId;
                return Task.FromResult(13);
            }

            var lazy = new CoreAsyncLazy<int>(Func);

            await lazy;

            Assert.NotEqual(testThread, funcThread);
        }

        [Fact]
        public async Task CoreAsyncLazy_Start_CallsFunc()
        {
            TaskCompletionSource<object> tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            Task<int> Func()
            {
                tcs.SetResult(null!);
                return Task.FromResult(13);
            }

            var lazy = new CoreAsyncLazy<int>(Func);

            lazy.Start();
            await tcs.Task;
        }

        [Fact]
        public async Task CoreAsyncLazy_Await_ReturnsFuncValue()
        {
            static async Task<int> Func()
            {
                await Task.Yield();
                return 13;
            }

            var lazy = new CoreAsyncLazy<int>(Func);

            int result = await lazy;
            Assert.Equal(13, result);
        }

        [Fact]
        public async Task CoreAsyncLazy_MultipleAwaiters_OnlyInvokeFuncOnce()
        {
            int invokeCount = 0;
            TaskCompletionSource<object> tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            async Task<int> Func()
            {
                Interlocked.Increment(ref invokeCount);
                await tcs.Task;
                return 13;
            }

            var lazy = new CoreAsyncLazy<int>(Func);

            Task<int> task1 = Task.Run(async () => await lazy);
            Task<int> task2 = Task.Run(async () => await lazy);

            Assert.False(task1.IsCompleted);
            Assert.False(task2.IsCompleted);
            tcs.SetResult(null!);
            int[] results = await Task.WhenAll(task1, task2);
            Assert.True(results.SequenceEqual(new[] { 13, 13 }));
            Assert.Equal(1, invokeCount);
        }

        [Fact]
        public async Task CoreAsyncLazy_FailureCachedByDefault()
        {
            int invokeCount = 0;
            async Task<int> Func()
            {
                Interlocked.Increment(ref invokeCount);
                await Task.Yield();
                return invokeCount == 1 ? throw new InvalidOperationException("Not today, punk.") : 13;
            }

            var lazy = new CoreAsyncLazy<int>(Func);
            await CoreAsyncAssert.ThrowsAsync<InvalidOperationException>(lazy.Task);

            await CoreAsyncAssert.ThrowsAsync<InvalidOperationException>(lazy.Task);
            Assert.Equal(1, invokeCount);
        }

        [Fact]
        public async Task CoreAsyncLazy_WithRetryOnFailure_DoesNotCacheFailure()
        {
            int invokeCount = 0;
            async Task<int> Func()
            {
                Interlocked.Increment(ref invokeCount);
                await Task.Yield();
                return invokeCount == 1 ? throw new InvalidOperationException("Not today, punk.") : 13;
            }

            var lazy = new CoreAsyncLazy<int>(Func, CoreAsyncLazyFlags.RetryOnFailure);
            await CoreAsyncAssert.ThrowsAsync<InvalidOperationException>(lazy.Task);

            Assert.Equal(13, await lazy);
            Assert.Equal(2, invokeCount);
        }

        [Fact]
        public async Task CoreAsyncLazy_WithRetryOnFailure_DoesNotRetryOnSuccess()
        {
            int invokeCount = 0;
            async Task<int> Func()
            {
                Interlocked.Increment(ref invokeCount);
                await Task.Yield();
                return invokeCount == 1 ? throw new InvalidOperationException("Not today, punk.") : 13;
            }

            var lazy = new CoreAsyncLazy<int>(Func, CoreAsyncLazyFlags.RetryOnFailure);
            await CoreAsyncAssert.ThrowsAsync<InvalidOperationException>(lazy.Task);

            await lazy;
            await lazy;

            Assert.Equal(13, await lazy);
            Assert.Equal(2, invokeCount);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var lazy = new CoreAsyncLazy<object>(() => Task.FromResult<object>(null!));
            Assert.NotEqual(0, lazy.Id);
        }
    }
}
