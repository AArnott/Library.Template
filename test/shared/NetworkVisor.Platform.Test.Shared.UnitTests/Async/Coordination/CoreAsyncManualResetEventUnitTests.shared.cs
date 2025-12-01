// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncManualResetEventUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreAsyncManualResetEventUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncManualResetEventUnitTests))]

    public class CoreAsyncManualResetEventUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncManualResetEventUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncManualResetEventUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task WaitAsync_Unset_IsNotCompleted()
        {
            var mre = new CoreAsyncManualResetEvent();

            Task? task = mre.WaitAsync();

            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task Wait_Unset_IsNotCompleted()
        {
            var mre = new CoreAsyncManualResetEvent();

            var task = Task.Run(mre.Wait);

            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public void WaitAsync_AfterSet_IsCompleted()
        {
            var mre = new CoreAsyncManualResetEvent();

            mre.Set();
            Task? task = mre.WaitAsync();

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void Wait_AfterSet_IsCompleted()
        {
            var mre = new CoreAsyncManualResetEvent();

            mre.Set();
            mre.Wait();
        }

        [Fact]
        public void WaitAsync_Set_IsCompleted()
        {
            var mre = new CoreAsyncManualResetEvent(true);

            Task? task = mre.WaitAsync();

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void Wait_Set_IsCompleted()
        {
            var mre = new CoreAsyncManualResetEvent(true);

            mre.Wait();
        }

        [Fact]
        public void MultipleWaitAsync_AfterSet_IsCompleted()
        {
            var mre = new CoreAsyncManualResetEvent();

            mre.Set();
            Task? task1 = mre.WaitAsync();
            Task? task2 = mre.WaitAsync();

            Assert.True(task1.IsCompleted);
            Assert.True(task2.IsCompleted);
        }

        [Fact]
        public void MultipleWait_AfterSet_IsCompleted()
        {
            var mre = new CoreAsyncManualResetEvent();

            mre.Set();
            mre.Wait();
            mre.Wait();
        }

        [Fact]
        public void MultipleWaitAsync_Set_IsCompleted()
        {
            var mre = new CoreAsyncManualResetEvent(true);

            Task? task1 = mre.WaitAsync();
            Task? task2 = mre.WaitAsync();

            Assert.True(task1.IsCompleted);
            Assert.True(task2.IsCompleted);
        }

        [Fact]
        public void MultipleWait_Set_IsCompleted()
        {
            var mre = new CoreAsyncManualResetEvent(true);

            mre.Wait();
            mre.Wait();
        }

        [Fact]
        public async Task WaitAsync_AfterReset_IsNotCompleted()
        {
            var mre = new CoreAsyncManualResetEvent();

            mre.Set();
            mre.Reset();
            Task? task = mre.WaitAsync();

            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task Wait_AfterReset_IsNotCompleted()
        {
            var mre = new CoreAsyncManualResetEvent();

            mre.Set();
            mre.Reset();
            var task = Task.Run(mre.Wait);

            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var mre = new CoreAsyncManualResetEvent();
            Assert.NotEqual(0, mre.Id);
        }
    }
}
