// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreCollectionDisposableUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreCollectionDisposableUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreCollectionDisposableUnitTests))]

    public class CoreCollectionDisposableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreCollectionDisposableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreCollectionDisposableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void Dispose_NullChild_DoesNotThrow()
        {
            var disposable = CollectionDisposable.Create((IDisposable?)null);
            disposable.Dispose();
        }

        [Fact]
        public void Dispose_DisposesChild()
        {
            bool actionInvoked = false;
            var disposable = CollectionDisposable.Create(new Disposable(() => { actionInvoked = true; }));
            disposable.Dispose();
            Assert.True(actionInvoked);
        }

        [Fact]
        public void Dispose_MultipleChildren_DisposesBothChildren()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var disposable = CollectionDisposable.Create(new Disposable(() => { action1Invoked = true; }), new Disposable(() => { action2Invoked = true; }));
            disposable.Dispose();
            Assert.True(action1Invoked);
            Assert.True(action2Invoked);
        }

        [Fact]
        public void Dispose_EnumerableChildren_DisposesAllChildren()
        {
            var action1Invoked = new BoolHolder();
            var action2Invoked = new BoolHolder();
            var disposable = CollectionDisposable.Create(new[] { action1Invoked, action2Invoked }.Select(bh => new Disposable(() => { bh.Value = true; })));
            disposable.Dispose();
            Assert.True(action1Invoked.Value);
            Assert.True(action2Invoked.Value);
        }

        [Fact]
        public void Dispose_AfterAdd_DisposesBothChildren()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var disposable = CollectionDisposable.Create(new Disposable(() => { action1Invoked = true; }));
            disposable.Add(new Disposable(() => { action2Invoked = true; }));
            disposable.Dispose();
            Assert.True(action1Invoked);
            Assert.True(action2Invoked);
        }

        [Fact]
        public void Dispose_AfterAddingNullChild_DoesNotThrow()
        {
            bool action1Invoked = false;
            var disposable = CollectionDisposable.Create(new Disposable(() => { action1Invoked = true; }));
            disposable.Add(null);
            disposable.Dispose();
            Assert.True(action1Invoked);
        }

        [Fact]
        public async Task Add_AfterDisposeStarts_DisposesNewChildAfterDisposalCompletes()
        {
            bool action1Invoked = false;
            bool action2Invoked = false;
            var ready = new ManualResetEventSlim();
            var signal = new ManualResetEventSlim();
            var disposable = CollectionDisposable.Create(new Disposable(() =>
            {
                action1Invoked = true;
                ready.Set();
                signal.Wait();
            }));
            var disposeTask = Task.Run(disposable.Dispose);
            ready.Wait();
            var addTask = Task.Run(() => disposable.Add(new Disposable(() => { action2Invoked = true; })));
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
            Assert.False(addTask.Wait(100));
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
            Assert.True(action1Invoked);
            Assert.False(action2Invoked);
            signal.Set();
            await disposeTask;
            await addTask;
            Assert.True(action2Invoked);
        }

        [Fact]
        public void MultipleDispose_OnlyDisposesChildOnce()
        {
            int counter = 0;
            var disposable = new CollectionDisposable(new Disposable(() => { ++counter; }));
            disposable.Dispose();
            disposable.Dispose();
            Assert.Equal(1, counter);
        }

        private sealed class BoolHolder
        {
            public bool Value { get; set; }
        }
    }
}
