// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreReferenceCountedAsyncDisposableUnitTests.shared.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Disposables
{
    /// <summary>
    /// Class CoreReferenceCountedAsyncDisposableUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreReferenceCountedAsyncDisposableUnitTests))]

    public class CoreReferenceCountedAsyncDisposableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreReferenceCountedAsyncDisposableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreReferenceCountedAsyncDisposableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task Create_NullDisposable_DoesNotThrow()
        {
            IReferenceCountedAsyncDisposable<IAsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create<IAsyncDisposable>(null);
            await disposable.DisposeAsync();
        }

        [Fact]
        public async Task AdvancedCreate_NullDisposable_DoesNotThrow()
        {
            IReferenceCountedAsyncDisposable<IAsyncDisposable> disposable = ReferenceCountedAsyncDisposable.CreateWithNewReferenceCounter<IAsyncDisposable>(null);
            await disposable.DisposeAsync();
        }

        [Fact]
        public void Target_ReturnsTarget()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            Assert.Equal(target, disposable.Target);
        }

        [Fact]
        public void Target_WhenNull_ReturnsTarget()
        {
            IReferenceCountedAsyncDisposable<IAsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create<IAsyncDisposable>(null);
            Assert.Null(disposable.Target);
        }

        [Fact]
        public async Task Target_AfterDispose_Throws()
        {
            IReferenceCountedAsyncDisposable<IAsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create<IAsyncDisposable>(null);
            await disposable.DisposeAsync();
            Assert.Throws<ObjectDisposedException>(() => disposable.Target);
        }

        [Fact]
        public async Task Target_WhenNull_AfterDispose_Throws()
        {
            IReferenceCountedAsyncDisposable<IAsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create<IAsyncDisposable>(null);
            await disposable.DisposeAsync();
            Assert.Throws<ObjectDisposedException>(() => disposable.Target);
        }

        [Fact]
        public async Task Dispose_DisposesTarget()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            Assert.False(target.IsDisposed);
            await disposable.DisposeAsync();
            Assert.True(target.IsDisposed);
        }

        [Fact]
        public async Task MultiDispose_DisposesTargetOnceAsync()
        {
            int targetDisposeCount = 0;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            var target = new UnsafeDisposable(async () => ++targetDisposeCount);
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            IReferenceCountedAsyncDisposable<UnsafeDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            Assert.Equal(0, targetDisposeCount);
            await disposable.DisposeAsync();
            Assert.Equal(1, targetDisposeCount);
            await disposable.DisposeAsync();
            Assert.Equal(1, targetDisposeCount);
        }

        [Fact]
        public async Task AddReference_AfterDispose_ThrowsAsync()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            await disposable.DisposeAsync();
            Assert.Throws<ObjectDisposedException>(disposable.AddReference);
        }

        [Fact]
        public async Task AddReference_AfterDispose_WhenAnotherReferenceExists_ThrowsAsync()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            IReferenceCountedAsyncDisposable<AsyncDisposable> secondDisposable = disposable.AddReference();
            await disposable.DisposeAsync();
            Assert.Throws<ObjectDisposedException>(disposable.AddReference);
            Assert.False(target.IsDisposed);
        }

        [Fact]
        public async Task Dispose_WhenAnotherReferenceExists_DoesNotDisposeTarget_UntilOtherReferenceIsDisposedAsync()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            IReferenceCountedAsyncDisposable<AsyncDisposable> secondDisposable = disposable.AddReference();
            Assert.False(target.IsDisposed);
            await disposable.DisposeAsync();
            Assert.False(target.IsDisposed);
            await secondDisposable.DisposeAsync();
            Assert.True(target.IsDisposed);
        }

        [Fact]
        public async Task MultiDispose_OnlyDecrementsReferenceCountOnceAsync()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            _ = disposable.AddReference();
            Assert.False(target.IsDisposed);
            await disposable.DisposeAsync();
            Assert.False(target.IsDisposed);
            await disposable.DisposeAsync();
            Assert.False(target.IsDisposed);
        }

        [Fact]
        public async Task MultiCreate_SameTarget_SharesReferenceCountAsync()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            IReferenceCountedAsyncDisposable<AsyncDisposable> secondDisposable = ReferenceCountedAsyncDisposable.Create(target);
            await disposable.DisposeAsync();
            Assert.False(target.IsDisposed);
            await secondDisposable.DisposeAsync();
            Assert.True(target.IsDisposed);
        }

        [Fact]
        public async Task MultiTryCreate_SameTarget_AfterDisposal_ReturnsNullAsync()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            await disposable.DisposeAsync();
            IReferenceCountedAsyncDisposable<AsyncDisposable>? secondDisposable = ReferenceCountedAsyncDisposable.TryCreate(target);
            Assert.Null(secondDisposable);
        }

        [Fact]
        public async Task MultiCreate_SameTarget_AfterDisposal_ThrowsAsync()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            await disposable.DisposeAsync();
            Assert.Throws<ObjectDisposedException>(() => ReferenceCountedAsyncDisposable.Create(target));
        }

        [Fact]
        public async Task AddWeakReference_AfterDispose_ThrowsAsync()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            await disposable.DisposeAsync();
            Assert.Throws<ObjectDisposedException>(disposable.AddWeakReference);
        }

        [Fact]
        public void WeakReferenceTarget_ReturnsTarget()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            IWeakReferenceCountedAsyncDisposable<AsyncDisposable> weakDisposable = disposable.AddWeakReference();
            Assert.Equal(target, weakDisposable.TryGetTarget());
            GC.KeepAlive(disposable);
        }

        [Fact]
        public async Task WeakReference_IsNotCountedAsync()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            IWeakReferenceCountedAsyncDisposable<AsyncDisposable> weakDisposable = disposable.AddWeakReference();
            await disposable.DisposeAsync();
            Assert.Null(weakDisposable.TryGetTarget());
            Assert.Null(weakDisposable.TryAddReference());
            GC.KeepAlive(disposable);
            GC.KeepAlive(target);
        }

        [Fact]
        public async Task WeakReference_NotDisposed_CanIncrementCountAsync()
        {
            var target = AsyncDisposable.Create(null);
            IReferenceCountedAsyncDisposable<AsyncDisposable> disposable = ReferenceCountedAsyncDisposable.Create(target);
            IWeakReferenceCountedAsyncDisposable<AsyncDisposable> weakDisposable = disposable.AddWeakReference();
            IReferenceCountedAsyncDisposable<AsyncDisposable>? secondDisposable = weakDisposable.TryAddReference();
            Assert.NotNull(secondDisposable);
            await disposable.DisposeAsync();
            Assert.NotNull(weakDisposable.TryGetTarget());
            Assert.False(target.IsDisposed);
            await secondDisposable.DisposeAsync();
            Assert.Null(weakDisposable.TryGetTarget());
            Assert.Null(weakDisposable.TryAddReference());
            GC.KeepAlive(secondDisposable);
            GC.KeepAlive(disposable);
            GC.KeepAlive(target);
        }

        [Fact]
        public void CreateDerived_AfterBase_RefersToSameTarget()
        {
            var target = new DerivedDisposable();
            var baseTarget = target as BaseDisposable;
            IReferenceCountedAsyncDisposable<BaseDisposable> baseDisposable = ReferenceCountedAsyncDisposable.Create(baseTarget);
            IReferenceCountedAsyncDisposable<DerivedDisposable> derivedDisposable = ReferenceCountedAsyncDisposable.Create(target);
            Assert.Equal(baseDisposable.Target, derivedDisposable.Target);
        }

        [Fact]
        public void CreateBase_AfterDerived_RefersToSameTarget()
        {
            var target = new DerivedDisposable();
            var baseTarget = target as BaseDisposable;
            IReferenceCountedAsyncDisposable<DerivedDisposable> derivedDisposable = ReferenceCountedAsyncDisposable.Create(target);
            IReferenceCountedAsyncDisposable<BaseDisposable> baseDisposable = ReferenceCountedAsyncDisposable.Create(baseTarget);
            Assert.Equal(baseDisposable.Target, derivedDisposable.Target);
        }

        [Fact]
        public void GenericVariance_RefersToSameTarget()
        {
            var target = new DerivedDisposable();
            IReferenceCountedAsyncDisposable<DerivedDisposable> derivedDisposable = ReferenceCountedAsyncDisposable.Create(target);
            var baseDisposable = derivedDisposable as IReferenceCountedAsyncDisposable<BaseDisposable>;
            Assert.NotNull(baseDisposable);
            Assert.Equal(baseDisposable.Target, derivedDisposable.Target);
        }

        [Fact]
        public void CastReferenceFromBaseToDerived_Fails()
        {
            var target = new DerivedDisposable();
            var baseTarget = target as BaseDisposable;
            IReferenceCountedAsyncDisposable<BaseDisposable> baseDisposable = ReferenceCountedAsyncDisposable.Create(baseTarget);
            var derivedDisposable = baseDisposable as IReferenceCountedAsyncDisposable<DerivedDisposable>;
            Assert.Null(derivedDisposable);
        }

        [Fact]
        public void CastTargetFromBaseToDerived_Succeeds()
        {
            var target = new DerivedDisposable();
            var baseTarget = target as BaseDisposable;
            IReferenceCountedAsyncDisposable<BaseDisposable> baseDisposable = ReferenceCountedAsyncDisposable.Create(baseTarget);
            var derivedTarget = baseDisposable.Target as DerivedDisposable;
            Assert.NotNull(derivedTarget);
            Assert.Equal(derivedTarget, target);
        }

        [Fact]
        public void Create_FromBothSynchronousAndAysnchronous_ReferencesSameTarget()
        {
            var target = new UnsafeSyncAsyncDisposable(() => { });
            IReferenceCountedDisposable<UnsafeSyncAsyncDisposable> syncDisposable = ReferenceCountedDisposable.Create(target);
            IReferenceCountedAsyncDisposable<UnsafeSyncAsyncDisposable> asyncDisposable = ReferenceCountedAsyncDisposable.Create(target);
            Assert.Equal(syncDisposable.Target, asyncDisposable.Target);
        }

        [Fact]
        public async Task BothSyncAndAysnc_SyncDisposedFirst_OnlyDisposesWhenBothAreDisposed()
        {
            int disposeCount = 0;
            var target = new UnsafeSyncAsyncDisposable(() => ++disposeCount);
            IReferenceCountedDisposable<UnsafeSyncAsyncDisposable> syncDisposable = ReferenceCountedDisposable.Create(target);
            IReferenceCountedAsyncDisposable<UnsafeSyncAsyncDisposable> asyncDisposable = ReferenceCountedAsyncDisposable.Create(target);
            syncDisposable.Dispose();
            Assert.Equal(0, disposeCount);
            await asyncDisposable.DisposeAsync();
            Assert.Equal(1, disposeCount);
        }

        [Fact]
        public async Task BothSyncAndAysnc_AsyncDisposedFirst_OnlyDisposesWhenBothAreDisposed()
        {
            int disposeCount = 0;
            var target = new UnsafeSyncAsyncDisposable(() => ++disposeCount);
            IReferenceCountedDisposable<UnsafeSyncAsyncDisposable> syncDisposable = ReferenceCountedDisposable.Create(target);
            IReferenceCountedAsyncDisposable<UnsafeSyncAsyncDisposable> asyncDisposable = ReferenceCountedAsyncDisposable.Create(target);
            await asyncDisposable.DisposeAsync();
            Assert.Equal(0, disposeCount);
            syncDisposable.Dispose();
            Assert.Equal(1, disposeCount);
        }

        private sealed class UnsafeDisposable : IAsyncDisposable
        {
            private readonly Func<Task> _action;

            public UnsafeDisposable(Func<Task> action) => this._action = action;

            public async ValueTask DisposeAsync() => await this._action();
        }

        private class BaseDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => default;
        }

        private class DerivedDisposable : BaseDisposable
        {
        }

        private sealed class UnsafeSyncAsyncDisposable : IDisposable, IAsyncDisposable
        {
            private readonly Action _action;

            public UnsafeSyncAsyncDisposable(Action action) => this._action = action;

            public void Dispose() => this._action();

            public ValueTask DisposeAsync()
            {
                this._action();
                return default;
            }
        }
    }
}
#endif
