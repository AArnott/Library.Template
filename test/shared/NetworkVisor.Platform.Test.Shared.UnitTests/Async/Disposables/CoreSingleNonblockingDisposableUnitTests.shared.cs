// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreSingleNonblockingDisposableUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Disposables;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Disposables
{
    /// <summary>
    /// Class CoreSingleNonblockingDisposableUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreSingleNonblockingDisposableUnitTests))]

    public class CoreSingleNonblockingDisposableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSingleNonblockingDisposableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSingleNonblockingDisposableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void Dispose_ConstructedWithContext_ReceivesThatContext()
        {
            object? providedContext = new();
            object? seenContext = null;
            var disposable = new DelegateSingleDisposable<object>(providedContext, context => { seenContext = context; });
            disposable.Dispose();
            Assert.Same(providedContext, seenContext);
        }

        [Fact]
        public void Dispose_UpdatedContext_ReceivesUpdatedContext()
        {
            object? originalContext = new();
            object? updatedContext = new();
            object? contextPassedToDispose = null;
            object? contextPassedToTryUpdateContextDelegate = null;
            var disposable = new DelegateSingleDisposable<object>(originalContext, context => { contextPassedToDispose = context; });
            Assert.True(disposable.TryUpdateContext(context =>
            {
                contextPassedToTryUpdateContextDelegate = context;
                return updatedContext;
            }));
            disposable.Dispose();
            Assert.Same(originalContext, contextPassedToTryUpdateContextDelegate);
            Assert.Same(updatedContext, contextPassedToDispose);
        }

        [Fact]
        public void TryUpdateContext_AfterDispose_ReturnsFalse()
        {
            object? originalContext = new();
            object? updatedContext = new();
            object? contextPassedToDispose = null;
            bool tryUpdateContextDelegateCalled = false;
            var disposable = new DelegateSingleDisposable<object>(originalContext, context => { contextPassedToDispose = context; });
            disposable.Dispose();
            Assert.False(disposable.TryUpdateContext(context =>
            {
                tryUpdateContextDelegateCalled = true;
                return updatedContext;
            }));
            Assert.False(tryUpdateContextDelegateCalled);
            Assert.Same(originalContext, contextPassedToDispose);
        }

        [Fact]
        public void DisposeOnlyCalledOnce()
        {
            int counter = 0;
            var disposable = new DelegateSingleDisposable<object>(new object(), _ => { ++counter; });
            disposable.Dispose();
            disposable.Dispose();
            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task DisposeIsNonblocking()
        {
            var ready = new ManualResetEventSlim();
            var signal = new ManualResetEventSlim();
            var disposable = new DelegateSingleDisposable<object>(new object(), _ =>
            {
                ready.Set();
                signal.Wait();
            });

            var task1 = Task.Run(disposable.Dispose);
            ready.Wait();

            await Task.Run(disposable.Dispose);

            signal.Set();
            await task1;
        }

        [Fact]
        public async Task LifetimeProperties_HaveAppropriateValues()
        {
            var ready = new ManualResetEventSlim();
            var signal = new ManualResetEventSlim();
            var disposable = new DelegateSingleDisposable<object>(new object(), _ =>
            {
                ready.Set();
                signal.Wait();
            });

            Assert.False(disposable.IsDisposed);

            var task1 = Task.Run(disposable.Dispose);
            ready.Wait();

            // Note: IsDisposed is true once disposal starts.
            Assert.True(disposable.IsDisposed);

            signal.Set();
            await task1;

            Assert.True(disposable.IsDisposed);
        }

        private sealed class DelegateSingleDisposable<T> : SingleNonblockingDisposable<T>
            where T : class
        {
            private readonly Action<T> _callback;

            public DelegateSingleDisposable(T context, Action<T> callback)
                : base(context)
            {
                this._callback = callback;
            }

            public new bool TryUpdateContext(Func<T, T> updater)
            {
                return base.TryUpdateContext(updater);
            }

            protected override void Dispose(T context)
            {
                this._callback(context);
            }
        }
    }
}
