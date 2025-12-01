// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncCountdownEventUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Coordination;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Coordination
{
    /// <summary>
    /// Class CoreAsyncCountdownEventUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncCountdownEventUnitTests))]

    public class CoreAsyncCountdownEventUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncCountdownEventUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncCountdownEventUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task WaitAsync_Unset_IsNotCompleted()
        {
            var ce = new CoreAsyncCountdownEvent(1);
            Task? task = ce.WaitAsync();

            Assert.Equal(1, ce.CurrentCount);
            Assert.False(task.IsCompleted);

            ce.Signal();
            await task;
        }

        [Fact]
        public void WaitAsync_Set_IsCompleted()
        {
            var ce = new CoreAsyncCountdownEvent(0);
            Task? task = ce.WaitAsync();

            Assert.Equal(0, ce.CurrentCount);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task AddCount_IncrementsCount()
        {
            var ce = new CoreAsyncCountdownEvent(1);
            Task? task = ce.WaitAsync();
            Assert.Equal(1, ce.CurrentCount);
            Assert.False(task.IsCompleted);

            ce.AddCount();

            Assert.Equal(2, ce.CurrentCount);
            Assert.False(task.IsCompleted);

            ce.Signal(2);
            await task;
        }

        [Fact]
        public async Task Signal_Nonzero_IsNotCompleted()
        {
            var ce = new CoreAsyncCountdownEvent(2);
            Task? task = ce.WaitAsync();
            Assert.False(task.IsCompleted);

            ce.Signal();

            Assert.Equal(1, ce.CurrentCount);
            Assert.False(task.IsCompleted);

            ce.Signal();
            await task;
        }

        [Fact]
        public void Signal_Zero_SynchronouslyCompletesWaitTask()
        {
            var ce = new CoreAsyncCountdownEvent(1);
            Task? task = ce.WaitAsync();
            Assert.False(task.IsCompleted);

            ce.Signal();

            Assert.Equal(0, ce.CurrentCount);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task Signal_AfterSet_CountsNegativeAndResetsTask()
        {
            var ce = new CoreAsyncCountdownEvent(0);
            Task? originalTask = ce.WaitAsync();

            ce.Signal();

            Task? newTask = ce.WaitAsync();
            Assert.Equal(-1, ce.CurrentCount);
            Assert.NotSame(originalTask, newTask);

            ce.AddCount();
            await newTask;
        }

        [Fact]
        public async Task AddCount_AfterSet_CountsPositiveAndResetsTask()
        {
            var ce = new CoreAsyncCountdownEvent(0);
            Task? originalTask = ce.WaitAsync();

            ce.AddCount();
            Task? newTask = ce.WaitAsync();

            Assert.Equal(1, ce.CurrentCount);
            Assert.NotSame(originalTask, newTask);

            ce.Signal();
            await newTask;
        }

        [Fact]
        public async Task Signal_PastZero_PulsesTask()
        {
            var ce = new CoreAsyncCountdownEvent(1);
            Task? originalTask = ce.WaitAsync();

            ce.Signal(2);
            await originalTask;
            Task? newTask = ce.WaitAsync();

            Assert.Equal(-1, ce.CurrentCount);
            Assert.NotSame(originalTask, newTask);

            ce.AddCount();
            await newTask;
        }

        [Fact]
        public async Task AddCount_PastZero_PulsesTask()
        {
            var ce = new CoreAsyncCountdownEvent(-1);
            Task? originalTask = ce.WaitAsync();

            ce.AddCount(2);
            await originalTask;
            Task? newTask = ce.WaitAsync();

            Assert.Equal(1, ce.CurrentCount);
            Assert.NotSame(originalTask, newTask);

            ce.Signal();
            await newTask;
        }

        [Fact]
        public void AddCount_Overflow_ThrowsException()
        {
            var ce = new CoreAsyncCountdownEvent(long.MaxValue);
            CoreAsyncAssert.Throws<OverflowException>(ce.AddCount);
        }

        [Fact]
        public void Signal_Underflow_ThrowsException()
        {
            var ce = new CoreAsyncCountdownEvent(long.MinValue);
            CoreAsyncAssert.Throws<OverflowException>(ce.Signal);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var ce = new CoreAsyncCountdownEvent(0);
            Assert.NotEqual(0, ce.Id);
        }
    }
}
