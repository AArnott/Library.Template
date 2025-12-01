// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreReferenceCountedDisposableUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreReferenceCountedDisposableUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreReferenceCountedDisposableUnitTests))]

    public class CoreReferenceCountedDisposableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreReferenceCountedDisposableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreReferenceCountedDisposableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void Create_NullDisposable_DoesNotThrow()
        {
            IReferenceCountedDisposable<IDisposable> disposable = ReferenceCountedDisposable.Create<IDisposable>(null);
            disposable.Dispose();
        }

        [Fact]
        public void AdvancedCreate_NullDisposable_DoesNotThrow()
        {
            IReferenceCountedDisposable<IDisposable> disposable = ReferenceCountedDisposable.CreateWithNewReferenceCounter<IDisposable>(null);
            disposable.Dispose();
        }

        [Fact]
        public void Target_ReturnsTarget()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            Assert.Equal(target, disposable.Target);
        }

        [Fact]
        public void Target_WhenNull_ReturnsTarget()
        {
            IReferenceCountedDisposable<IDisposable> disposable = ReferenceCountedDisposable.Create<IDisposable>(null);
            Assert.Null(disposable.Target);
        }

        [Fact]
        public void Target_AfterDispose_Throws()
        {
            IReferenceCountedDisposable<IDisposable> disposable = ReferenceCountedDisposable.Create<IDisposable>(null);
            disposable.Dispose();
            Assert.Throws<ObjectDisposedException>(() => disposable.Target);
        }

        [Fact]
        public void Target_WhenNull_AfterDispose_Throws()
        {
            IReferenceCountedDisposable<IDisposable> disposable = ReferenceCountedDisposable.Create<IDisposable>(null);
            disposable.Dispose();
            Assert.Throws<ObjectDisposedException>(() => disposable.Target);
        }

        [Fact]
        public void Dispose_DisposesTarget()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            Assert.False(target.IsDisposed);
            disposable.Dispose();
            Assert.True(target.IsDisposed);
        }

        [Fact]
        public void MultiDispose_DisposesTargetOnce()
        {
            int targetDisposeCount = 0;
            var target = new UnsafeDisposable(() => ++targetDisposeCount);
            IReferenceCountedDisposable<UnsafeDisposable> disposable = ReferenceCountedDisposable.Create(target);
            Assert.Equal(0, targetDisposeCount);
            disposable.Dispose();
            Assert.Equal(1, targetDisposeCount);
            disposable.Dispose();
            Assert.Equal(1, targetDisposeCount);
        }

        [Fact]
        public void AddReference_AfterDispose_Throws()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            disposable.Dispose();
            Assert.Throws<ObjectDisposedException>(disposable.AddReference);
        }

        [Fact]
        public void AddReference_AfterDispose_WhenAnotherReferenceExists_Throws()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            IReferenceCountedDisposable<Disposable> secondDisposable = disposable.AddReference();
            disposable.Dispose();
            Assert.Throws<ObjectDisposedException>(disposable.AddReference);
            Assert.False(target.IsDisposed);
        }

        [Fact]
        public void Dispose_WhenAnotherReferenceExists_DoesNotDisposeTarget_UntilOtherReferenceIsDisposed()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            IReferenceCountedDisposable<Disposable> secondDisposable = disposable.AddReference();
            Assert.False(target.IsDisposed);
            disposable.Dispose();
            Assert.False(target.IsDisposed);
            secondDisposable.Dispose();
            Assert.True(target.IsDisposed);
        }

        [Fact]
        public void MultiDispose_OnlyDecrementsReferenceCountOnce()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            _ = disposable.AddReference();
            Assert.False(target.IsDisposed);
            disposable.Dispose();
            Assert.False(target.IsDisposed);
            disposable.Dispose();
            Assert.False(target.IsDisposed);
        }

        [Fact]
        public void MultiCreate_SameTarget_SharesReferenceCount()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            IReferenceCountedDisposable<Disposable> secondDisposable = ReferenceCountedDisposable.Create(target);
            disposable.Dispose();
            Assert.False(target.IsDisposed);
            secondDisposable.Dispose();
            Assert.True(target.IsDisposed);
        }

        [Fact]
        public void MultiTryCreate_SameTarget_AfterDisposal_ReturnsNull()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            disposable.Dispose();
            IReferenceCountedDisposable<Disposable>? secondDisposable = ReferenceCountedDisposable.TryCreate(target);
            Assert.Null(secondDisposable);
        }

        [Fact]
        public void MultiCreate_SameTarget_AfterDisposal_Throws()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            disposable.Dispose();
            Assert.Throws<ObjectDisposedException>(() => ReferenceCountedDisposable.Create(target));
        }

        [Fact]
        public void AddWeakReference_AfterDispose_Throws()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            disposable.Dispose();
            Assert.Throws<ObjectDisposedException>(disposable.AddWeakReference);
        }

        [Fact]
        public void WeakReferenceTarget_ReturnsTarget()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            IWeakReferenceCountedDisposable<Disposable> weakDisposable = disposable.AddWeakReference();
            Assert.Equal(target, weakDisposable.TryGetTarget());
            GC.KeepAlive(disposable);
        }

        [Fact]
        public void WeakReference_IsNotCounted()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            IWeakReferenceCountedDisposable<Disposable> weakDisposable = disposable.AddWeakReference();
            disposable.Dispose();
            Assert.Null(weakDisposable.TryGetTarget());
            Assert.Null(weakDisposable.TryAddReference());
            GC.KeepAlive(disposable);
            GC.KeepAlive(target);
        }

        [Fact]
        public void WeakReference_NotDisposed_CanIncrementCount()
        {
            var target = Disposable.Create(null);
            IReferenceCountedDisposable<Disposable> disposable = ReferenceCountedDisposable.Create(target);
            IWeakReferenceCountedDisposable<Disposable> weakDisposable = disposable.AddWeakReference();
            IReferenceCountedDisposable<Disposable>? secondDisposable = weakDisposable.TryAddReference();
            Assert.NotNull(secondDisposable);
            disposable.Dispose();
            Assert.NotNull(weakDisposable.TryGetTarget());
            Assert.False(target.IsDisposed);
            secondDisposable.Dispose();
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
            IReferenceCountedDisposable<BaseDisposable> baseDisposable = ReferenceCountedDisposable.Create(baseTarget);
            IReferenceCountedDisposable<DerivedDisposable> derivedDisposable = ReferenceCountedDisposable.Create(target);
            Assert.Equal(baseDisposable.Target, derivedDisposable.Target);
        }

        [Fact]
        public void CreateBase_AfterDerived_RefersToSameTarget()
        {
            var target = new DerivedDisposable();
            var baseTarget = target as BaseDisposable;
            IReferenceCountedDisposable<DerivedDisposable> derivedDisposable = ReferenceCountedDisposable.Create(target);
            IReferenceCountedDisposable<BaseDisposable> baseDisposable = ReferenceCountedDisposable.Create(baseTarget);
            Assert.Equal(baseDisposable.Target, derivedDisposable.Target);
        }

        [Fact]
        public void GenericVariance_RefersToSameTarget()
        {
            var target = new DerivedDisposable();
            IReferenceCountedDisposable<DerivedDisposable> derivedDisposable = ReferenceCountedDisposable.Create(target);
            var baseDisposable = derivedDisposable as IReferenceCountedDisposable<BaseDisposable>;
            Assert.NotNull(baseDisposable);
            Assert.Equal(baseDisposable.Target, derivedDisposable.Target);
        }

        [Fact]
        public void CastReferenceFromBaseToDerived_Fails()
        {
            var target = new DerivedDisposable();
            var baseTarget = target as BaseDisposable;
            IReferenceCountedDisposable<BaseDisposable> baseDisposable = ReferenceCountedDisposable.Create(baseTarget);
            var derivedDisposable = baseDisposable as IReferenceCountedDisposable<DerivedDisposable>;
            Assert.Null(derivedDisposable);
        }

        [Fact]
        public void CastTargetFromBaseToDerived_Succeeds()
        {
            var target = new DerivedDisposable();
            var baseTarget = target as BaseDisposable;
            IReferenceCountedDisposable<BaseDisposable> baseDisposable = ReferenceCountedDisposable.Create(baseTarget);
            var derivedTarget = baseDisposable.Target as DerivedDisposable;
            Assert.NotNull(derivedTarget);
            Assert.Equal(derivedTarget, target);
        }

        private sealed class UnsafeDisposable : IDisposable
        {
            private readonly Action _action;

            public UnsafeDisposable(Action action) => this._action = action;

            public void Dispose() => this._action();
        }

        private class BaseDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private class DerivedDisposable : BaseDisposable
        {
        }
    }
}
