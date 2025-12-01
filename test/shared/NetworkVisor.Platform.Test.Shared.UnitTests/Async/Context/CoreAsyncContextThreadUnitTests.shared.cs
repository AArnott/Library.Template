// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncContextThreadUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Context;
using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Context
{
    /// <summary>
    /// Class CoreAsyncContextThreadUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncContextThreadUnitTests))]

    public class CoreAsyncContextThreadUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncContextThreadUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncContextThreadUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task AsyncContextThread_IsAnIndependentThread()
        {
            int testThread = Thread.CurrentThread.ManagedThreadId;
            var thread = new CoreAsyncContextThread();
            int contextThread = await thread.Factory.Run(() => Thread.CurrentThread.ManagedThreadId);
            Assert.NotEqual(testThread, contextThread);
            await thread.JoinAsync();
        }

        [Fact]
        public async Task AsyncDelegate_ResumesOnSameThread()
        {
            var thread = new CoreAsyncContextThread();
            int contextThread = -1, resumeThread = -1;
            await thread.Factory.Run(async () =>
            {
                contextThread = Thread.CurrentThread.ManagedThreadId;
                await Task.Yield();
                resumeThread = Thread.CurrentThread.ManagedThreadId;
            });
            Assert.Equal(contextThread, resumeThread);
            await thread.JoinAsync();
        }

        [Fact]
        public async Task Join_StopsTask()
        {
            var context = new CoreAsyncContextThread();
            Thread thread = await context.Factory.Run(() => Thread.CurrentThread);
            await context.JoinAsync();
        }

        [Fact]
        public async Task Context_IsCorrectAsyncContext()
        {
            using var thread = new CoreAsyncContextThread();
            CoreAsyncContext? observedContext = await thread.Factory.Run(() => CoreAsyncContext.Current);
            Assert.Same(observedContext, thread.Context);
        }
    }
}
