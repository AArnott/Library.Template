// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncConditionVariableUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreAsyncConditionVariableUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncConditionVariableUnitTests))]

    public class CoreAsyncConditionVariableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncConditionVariableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncConditionVariableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task WaitAsync_WithoutNotify_IsNotCompleted()
        {
            var mutex = new CoreAsyncLock();
            var cv = new CoreAsyncConditionVariable(mutex);

            await mutex.LockAsync();
            Task? task = cv.WaitAsync();

            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task WaitAsync_Notified_IsCompleted()
        {
            var mutex = new CoreAsyncLock();
            var cv = new CoreAsyncConditionVariable(mutex);
            await mutex.LockAsync();
            Task? task = cv.WaitAsync();

            await Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    cv.Notify();
                }
            });
            await task;
        }

        [Fact]
        public async Task WaitAsync_AfterNotify_IsNotCompleted()
        {
            var mutex = new CoreAsyncLock();
            var cv = new CoreAsyncConditionVariable(mutex);
            await Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    cv.Notify();
                }
            });

            await mutex.LockAsync();
            Task? task = cv.WaitAsync();

            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task MultipleWaits_NotifyAll_AllAreCompleted()
        {
            var mutex = new CoreAsyncLock();
            var cv = new CoreAsyncConditionVariable(mutex);
            IDisposable? key1 = await mutex.LockAsync();
            Task? task1 = cv.WaitAsync();
            Task? t_ = task1.ContinueWith(_ => key1.Dispose());
            IDisposable? key2 = await mutex.LockAsync();
            Task? task2 = cv.WaitAsync();
            Task? tt_ = task2.ContinueWith(_ => key2.Dispose());

            await Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    cv.NotifyAll();
                }
            });

            await task1;
            await task2;
        }

        [Fact]
        public async Task MultipleWaits_Notify_OneIsCompleted()
        {
            var mutex = new CoreAsyncLock();
            var cv = new CoreAsyncConditionVariable(mutex);
            IDisposable? key = await mutex.LockAsync();
            Task? task1 = cv.WaitAsync();
            Task? t_ = task1.ContinueWith(_ => key.Dispose());
            await mutex.LockAsync();
            Task? task2 = cv.WaitAsync();

            await Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    cv.Notify();
                }
            });

            await task1;
            await CoreAsyncAssert.NeverCompletesAsync(task2);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var mutex = new CoreAsyncLock();
            var cv = new CoreAsyncConditionVariable(mutex);
            Assert.NotEqual(0, cv.Id);
        }
    }
}
