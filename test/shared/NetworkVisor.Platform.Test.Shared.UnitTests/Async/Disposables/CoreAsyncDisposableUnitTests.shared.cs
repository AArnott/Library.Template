// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncDisposableUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER

using System;
using NetworkVisor.Core.Async.Coordination;
using NetworkVisor.Core.Async.Disposables;
using NetworkVisor.Core.Async.Tasks.Interop;
using NetworkVisor.Core.Async.Tasks.Synchronous;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Disposables
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    /// <summary>
    /// Class CoreAsyncDisposableUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncDisposableUnitTests))]

    public class CoreAsyncDisposableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncDisposableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncDisposableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task Dispose_NullAction_DoesNotThrow()
        {
            var disposable = AsyncDisposable.Create(null);
            await disposable.DisposeAsync();
        }

        [Fact]
        public async Task Dispose_InvokesAction()
        {
            bool actionInvoked = false;
            var disposable = AsyncDisposable.Create(async () => { actionInvoked = true; });
            await disposable.DisposeAsync();
            Assert.True(actionInvoked);
        }

        [Fact]
        public async Task Dispose_AfterAdd_InvokesBothActions()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var disposable = AsyncDisposable.Create(async () => { action1Invoked = true; });
            await disposable.AddAsync(async () => { action2Invoked = true; });
            await disposable.DisposeAsync();
            Assert.True(action1Invoked);
            Assert.True(action2Invoked);
        }

        [Fact]
        public async Task Dispose_AfterAddingNull_DoesNotThrow()
        {
            bool action1Invoked = false;
            var disposable = AsyncDisposable.Create(async () => { action1Invoked = true; });
            await disposable.AddAsync(null);
            await disposable.DisposeAsync();
            Assert.True(action1Invoked);
        }

        [Fact]
        public async Task Add_AfterDisposeStarts_ExecutingConcurrently_InvokesActionImmediately()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var ready = new TaskCompletionSource<object>();
            var signal = new TaskCompletionSource<object>();
            var disposable = new AsyncDisposable(
                async () =>
            {
                _ = ready.TrySetResult(null!);
                _ = await signal.Task;
                action1Invoked = true;
            },
                AsyncDisposeFlags.ExecuteConcurrently);
            var task = Task.Run(async () => await disposable.DisposeAsync());
            _ = await ready.Task;
            await disposable.AddAsync(async () => { action2Invoked = true; });
            Assert.False(action1Invoked);
            Assert.True(action2Invoked);
            _ = signal.TrySetResult(null!);
            await task;
            Assert.True(action1Invoked);
        }

        [Fact]
        public async Task Add_AfterDisposeStarts_ExecutingInSerial_InvokesActionAfterDisposeCompletes()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var ready = new TaskCompletionSource<object>();
            var signal = new TaskCompletionSource<object>();
            var disposable = AsyncDisposable.Create(async () =>
            {
                action1Invoked = true;
                _ = ready.TrySetResult(null!);
                _ = await signal.Task;
            });
            var disposeTask = Task.Run(async () => await disposable.DisposeAsync());
            _ = await ready.Task;
            var addTask = Task.Run(async () => await disposable.AddAsync(async () => { action2Invoked = true; }));
            Assert.NotEqual(addTask, await Task.WhenAny(addTask, Task.Delay(100)));
            Assert.True(action1Invoked);
            Assert.False(action2Invoked);
            _ = signal.TrySetResult(null!);
            await disposeTask;
            await addTask;
            Assert.True(action2Invoked);
        }

        [Fact]
        public async Task Actions_ExecutingInSerial_ExecuteInSerial()
        {
            bool running = false;
            var disposable = new AsyncDisposable(null);
            for (int i = 0; i != 10; ++i)
            {
                await disposable.AddAsync(async () =>
                {
                    Assert.False(running);
                    running = true;
                    await Task.Delay(10).ConfigureAwait(false);
                    running = false;
                });
            }

            await disposable.DisposeAsync();
        }

        [Fact]
        public async Task MultipleDispose_OnlyInvokesActionOnce()
        {
            int counter = 0;
            var disposable = AsyncDisposable.Create(async () => { ++counter; });
            await disposable.DisposeAsync();
            await disposable.DisposeAsync();
            Assert.Equal(1, counter);
        }
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
#endif
