// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreDeferralManagerUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Oop;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Oop
{
    /// <summary>
    /// Class CoreDeferralManagerUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreDeferralManagerUnitTests))]

    public class CoreDeferralManagerUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreDeferralManagerUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreDeferralManagerUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void NoDeferrals_IsCompleted()
        {
            var dm = new DeferralManager();
            Task task = dm.WaitForDeferralsAsync();
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task IncompleteDeferral_PreventsCompletion()
        {
            var dm = new DeferralManager();
            IDisposable deferral = dm.DeferralSource.GetDeferral();
            await CoreAsyncAssert.NeverCompletesAsync(dm.WaitForDeferralsAsync());
        }

        [Fact]
        public async Task DeferralCompleted_Completes()
        {
            var dm = new DeferralManager();
            IDisposable deferral = dm.DeferralSource.GetDeferral();
            Task task = dm.WaitForDeferralsAsync();
            Assert.False(task.IsCompleted);
            deferral.Dispose();
            await task;
        }

        [Fact]
        public async Task MultipleDeferralsWithOneIncomplete_PreventsCompletion()
        {
            var dm = new DeferralManager();
            IDisposable deferral1 = dm.DeferralSource.GetDeferral();
            IDisposable deferral2 = dm.DeferralSource.GetDeferral();
            Task task = dm.WaitForDeferralsAsync();
            deferral1.Dispose();
            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task TwoDeferralsWithOneCompletedTwice_PreventsCompletion()
        {
            var dm = new DeferralManager();
            IDisposable deferral1 = dm.DeferralSource.GetDeferral();
            IDisposable deferral2 = dm.DeferralSource.GetDeferral();
            Task task = dm.WaitForDeferralsAsync();
            deferral1.Dispose();
            deferral1.Dispose();
            await CoreAsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task MultipleDeferralsWithAllCompleted_Completes()
        {
            var dm = new DeferralManager();
            IDisposable deferral1 = dm.DeferralSource.GetDeferral();
            IDisposable deferral2 = dm.DeferralSource.GetDeferral();
            Task task = dm.WaitForDeferralsAsync();
            deferral1.Dispose();
            deferral2.Dispose();
            await task;
        }

        [Fact]
        public async Task CompletedDeferralFollowedByIncompleteDeferral_PreventsCompletion()
        {
            var dm = new DeferralManager();
            dm.DeferralSource.GetDeferral().Dispose();
            IDisposable deferral = dm.DeferralSource.GetDeferral();
            Task task = dm.WaitForDeferralsAsync();
            await CoreAsyncAssert.NeverCompletesAsync(task);
        }
    }
}
