// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreCollectionAsyncDisposableUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using System;
using System.Threading.Tasks;
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
    /// <summary>
    /// Class CoreCollectionAsyncDisposableUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreCollectionAsyncDisposableUnitTests))]

    public class CoreCollectionAsyncDisposableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreCollectionAsyncDisposableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreCollectionAsyncDisposableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task Dispose_NullChild_DoesNotThrow()
        {
            var disposable = CollectionAsyncDisposable.Create((IAsyncDisposable?)null);
            await disposable.DisposeAsync();
        }

        [Fact]
        public async Task Dispose_DisposesChild()
        {
            bool actionInvoked = false;
            var disposable = CollectionAsyncDisposable.Create(new AsyncDisposable(async () => { actionInvoked = true; }));
            await disposable.DisposeAsync();
            Assert.True(actionInvoked);
        }

        [Fact]
        public async Task Dispose_MultipleChildren_DisposesBothChildren()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var disposable = CollectionAsyncDisposable.Create(new AsyncDisposable(async () => { action1Invoked = true; }), new AsyncDisposable(async () => { action2Invoked = true; }));
            await disposable.DisposeAsync();
            Assert.True(action1Invoked);
            Assert.True(action2Invoked);
        }

        [Fact]
        public async Task Dispose_EnumerableChildren_DisposesAllChildren()
        {
            var action1Invoked = new BoolHolder();
            var action2Invoked = new BoolHolder();
            var disposable = CollectionAsyncDisposable.Create(new[] { action1Invoked, action2Invoked }.Select(bh => new AsyncDisposable(async () => { bh.Value = true; })));
            await disposable.DisposeAsync();
            Assert.True(action1Invoked.Value);
            Assert.True(action2Invoked.Value);
        }

        [Fact]
        public async Task Dispose_AfterAdd_DisposesBothChildren()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var disposable = CollectionAsyncDisposable.Create(new AsyncDisposable(async () => { action1Invoked = true; }));
            await disposable.AddAsync(new AsyncDisposable(async () => { action2Invoked = true; }));
            await disposable.DisposeAsync();
            Assert.True(action1Invoked);
            Assert.True(action2Invoked);
        }

        [Fact]
        public async Task Dispose_AfterAddingNullChild_DoesNotThrow()
        {
            bool action1Invoked = false;
            var disposable = CollectionAsyncDisposable.Create(new AsyncDisposable(async () => { action1Invoked = true; }));
            await disposable.AddAsync(null);
            await disposable.DisposeAsync();
            Assert.True(action1Invoked);
        }

        [Fact]
        public async Task AllowsMixedChildren()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var disposable = CollectionAsyncDisposable.Create(
                new AsyncDisposable(async () => { action1Invoked = true; }),
                Disposable.Create(() => action2Invoked = true).ToAsyncDisposable());
            await disposable.DisposeAsync();
            Assert.True(action1Invoked);
            Assert.True(action2Invoked);
        }

        [Fact]
        public async Task Add_AfterDisposeStarts_ExecutingConcurrently_InvokesActionImmediately()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var ready = new TaskCompletionSource<object>();
            var signal = new TaskCompletionSource<object>();
            var disposable = new CollectionAsyncDisposable(
                new[]
            {
                new AsyncDisposable(async () =>
                {
                    ready.TrySetResult(null!);
                    await signal.Task;
                    action1Invoked = true;
                }),
            },
                AsyncDisposeFlags.ExecuteConcurrently);
            var task = Task.Run(async () => await disposable.DisposeAsync());
            await ready.Task;
            await disposable.AddAsync(new AsyncDisposable(async () => { action2Invoked = true; }));
            Assert.False(action1Invoked);
            Assert.True(action2Invoked);
            signal.TrySetResult(null!);
            await task;
            Assert.True(action1Invoked);
        }

        [Fact]
        public async Task Add_AfterDisposeStarts_ExecutingInSerial_DisposesNewChildAfterDisposalCompletes()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var ready = new TaskCompletionSource<object>();
            var signal = new TaskCompletionSource<object>();
            var disposable = CollectionAsyncDisposable.Create(new AsyncDisposable(async () =>
            {
                action1Invoked = true;
                ready.TrySetResult(null!);
                await signal.Task;
            }));
            var disposeTask = Task.Run(async () => await disposable.DisposeAsync());
            await ready.Task;
            var addTask = Task.Run(async () => await disposable.AddAsync(new AsyncDisposable(async () => { action2Invoked = true; })));
            Assert.NotEqual(addTask, await Task.WhenAny(addTask, Task.Delay(100)));
            Assert.True(action1Invoked);
            Assert.False(action2Invoked);
            signal.TrySetResult(null!);
            await disposeTask;
            await addTask;
            Assert.True(action2Invoked);
        }

        [Fact]
        public async Task Children_ExecutingInSerial_ExecuteInSerial()
        {
            bool running = false;
            var disposable = new CollectionAsyncDisposable();
            for (int i = 0; i != 10; ++i)
            {
                await disposable.AddAsync(new AsyncDisposable(async () =>
                {
                    Assert.False(running);
                    running = true;
                    await Task.Delay(10);
                    running = false;
                }));
            }

            await disposable.DisposeAsync();
        }

        [Fact]
        public async Task MultipleDispose_OnlyDisposesChildOnce()
        {
            int counter = 0;
            var disposable = new CollectionAsyncDisposable(new AsyncDisposable(async () => { ++counter; }));
            await disposable.DisposeAsync();
            await disposable.DisposeAsync();
            Assert.Equal(1, counter);
        }

        private sealed class BoolHolder
        {
            public bool Value { get; set; }
        }
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#endif
