// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreSingleAsyncDisposableUnitTests.shared.cs" company="Network Visor">
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Disposables
{
    /// <summary>
    /// Class CoreSingleAsyncDisposableUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreSingleAsyncDisposableUnitTests))]

    public class CoreSingleAsyncDisposableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSingleAsyncDisposableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSingleAsyncDisposableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task Dispose_ConstructedWithContext_ReceivesThatContext()
        {
            object? providedContext = new();
            object? seenContext = null;
            var disposable = new DelegateSingleDisposable<object>(providedContext, async context => { seenContext = context; });
            await disposable.DisposeAsync();
            Assert.Same(providedContext, seenContext);
        }

        [Fact]
        public async Task Dispose_UpdatedContext_ReceivesUpdatedContext()
        {
            object? originalContext = new();
            object? updatedContext = new();
            object? contextPassedToDispose = null;
            object? contextPassedToTryUpdateContextDelegate = null;
            var disposable = new DelegateSingleDisposable<object>(originalContext, async context => { contextPassedToDispose = context; });
            Assert.True(disposable.TryUpdateContext(context =>
            {
                contextPassedToTryUpdateContextDelegate = context;
                return updatedContext;
            }));
            await disposable.DisposeAsync();
            Assert.Same(originalContext, contextPassedToTryUpdateContextDelegate);
            Assert.Same(updatedContext, contextPassedToDispose);
        }

        [Fact]
        public async Task TryUpdateContext_AfterDisposeStarts_ReturnsFalse()
        {
            object? originalContext = new();
            object? updatedContext = new();
            object? contextPassedToDispose = null;
            bool tryUpdateContextDelegateCalled = false;
            var ready = new TaskCompletionSource<object>();
            var signal = new TaskCompletionSource<object>();
            var disposable = new DelegateSingleDisposable<object>(originalContext, async context =>
            {
                ready.TrySetResult(null!);
                await signal.Task;
                contextPassedToDispose = context;
            });
            var task = Task.Run(async () => await disposable.DisposeAsync());
            await ready.Task;
            Assert.False(disposable.TryUpdateContext(context =>
            {
                tryUpdateContextDelegateCalled = true;
                return updatedContext;
            }));
            signal.TrySetResult(null!);
            await task;
            Assert.False(tryUpdateContextDelegateCalled);
            Assert.Same(originalContext, contextPassedToDispose);
        }

        [Fact]
        public async Task TryUpdateContext_AfterDisposeCompletes_ReturnsFalse()
        {
            object? originalContext = new();
            object? updatedContext = new();
            object? contextPassedToDispose = null;
            bool tryUpdateContextDelegateCalled = false;
            var disposable = new DelegateSingleDisposable<object>(originalContext, async context => { contextPassedToDispose = context; });
            await disposable.DisposeAsync();
            Assert.False(disposable.TryUpdateContext(context =>
            {
                tryUpdateContextDelegateCalled = true;
                return updatedContext;
            }));
            Assert.False(tryUpdateContextDelegateCalled);
            Assert.Same(originalContext, contextPassedToDispose);
        }

        [Fact]
        public async Task DisposeOnlyCalledOnce()
        {
            int counter = 0;
            var disposable = new DelegateSingleDisposable<object>(new object(), async _ => { ++counter; });
            await disposable.DisposeAsync();
            await disposable.DisposeAsync();
            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task DisposableWaitsForDisposeToComplete()
        {
            var ready = new TaskCompletionSource<object>();
            var signal = new TaskCompletionSource<object>();
            var disposable = new DelegateSingleDisposable<object>(new object(), async _ =>
            {
                ready.TrySetResult(null!);
                await signal.Task;
            });

            var task1 = Task.Run(async () => await disposable.DisposeAsync());
            await ready.Task;

            var task2 = Task.Run(async () => await disposable.DisposeAsync());
            var timer = Task.Delay(500);
            Assert.Same(timer, await Task.WhenAny(task1, task2, timer));

            signal.TrySetResult(null!);
            await task1;
            await task2;
        }

        [Fact]
        public async Task LifetimeProperties_HaveAppropriateValues()
        {
            var ready = new TaskCompletionSource<object>();
            var signal = new TaskCompletionSource<object>();

            var disposable = new DelegateSingleDisposable<object>(new object(), async _ =>
            {
                ready.TrySetResult(null!);
                await signal.Task;
            });

            Assert.False(disposable.IsDisposing);
            Assert.False(disposable.IsDisposeStarted);
            Assert.False(disposable.IsDisposed);

            var task1 = Task.Run(async () => await disposable.DisposeAsync());
            await ready.Task;

            Assert.True(disposable.IsDisposing);
            Assert.True(disposable.IsDisposeStarted);
            Assert.False(disposable.IsDisposed);

            signal.TrySetResult(null!);
            await task1;

            Assert.False(disposable.IsDisposing);
            Assert.True(disposable.IsDisposeStarted);
            Assert.True(disposable.IsDisposed);
        }

        private sealed class DelegateSingleDisposable<T> : SingleAsyncDisposable<T>
            where T : class
        {
            private readonly Func<T, Task> _callback;

            public DelegateSingleDisposable(T context, Func<T, Task> callback)
                : base(context)
            {
                this._callback = callback;
            }

            public new bool TryUpdateContext(Func<T, T> updater)
            {
                return base.TryUpdateContext(updater);
            }

            protected override async ValueTask DisposeAsync(T context)
            {
                await this._callback(context);
            }
        }
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#endif
