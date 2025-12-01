// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreCachedTaskBaseIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Core Cached Task Base Integration Tests.</summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Async
{
    /// <summary>
    /// Class CoreCachedTaskBaseIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreCachedTaskBaseIntegrationTests))]

    public class CoreCachedTaskBaseIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreCachedTaskBaseIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreCachedTaskBaseIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [InlineData(CoreTaskCacheStateFlags.Current)]
        [InlineData(CoreTaskCacheStateFlags.Error)]
        [InlineData(CoreTaskCacheStateFlags.NotInitialized)]
        [InlineData(CoreTaskCacheStateFlags.UpdateInProgress)]
        public void CachedTaskBaseIntegration_InitialState(CoreTaskCacheStateFlags taskCacheStateFlags)
        {
            // Success: No Cache expiration. Initial cache state.
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, taskCacheStateFlags);

            testCoreCachedTask.IsCacheCurrent.Should().Be(taskCacheStateFlags == CoreTaskCacheStateFlags.Current);
            testCoreCachedTask.IsCacheInvalid.Should().Be(taskCacheStateFlags == CoreTaskCacheStateFlags.NotInitialized || taskCacheStateFlags == CoreTaskCacheStateFlags.Error || testCoreCachedTask.IsCacheNull);
            testCoreCachedTask.IsCacheNotInitialized.Should().Be(taskCacheStateFlags == CoreTaskCacheStateFlags.NotInitialized);
            testCoreCachedTask.IsCacheUpdating.Should().Be(taskCacheStateFlags == CoreTaskCacheStateFlags.UpdateInProgress);
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_ExpireRefreshAsync()
        {
            // Create a cache that expires in 2 seconds.
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.QuickCacheExpiration, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.Current);

            await CoreCacheTestExtensions.ValidateCacheTask_TestExpireRefreshAsync(testCoreCachedTask, CoreCacheTestExtensions.StringKeyValue, CoreTaskCacheStateFlags.Current, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_SuccessAsync()
        {
            // Success: No Cache expiration, task completes before operation timeout
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<string?> result = await testCoreCachedTask.TestRefreshCacheAsync(() => CoreCacheTestExtensions.TestDelayTask<string?>("Success", this.TestCaseLogger), CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout, CancellationToken.None);

            CoreCacheTestExtensions.ValidateCacheResult_Success(result, CoreTaskCacheStateFlags.NotInitialized, false, this.TestOutputHelper, this.TestCaseLogger);

            result.Result.Should().Be("Success");
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_DefaultTimeoutAsync()
        {
            // Success: No Cache expiration, task completes before operation timeout
            var testCoreCachedTask = new TestCoreCachedTask<string?>(this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<string?> result = await testCoreCachedTask.TestRefreshCacheAsync(() => CoreCacheTestExtensions.TestDelayTask<string?>("Success", this.TestCaseLogger), CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout, CancellationToken.None);

            CoreCacheTestExtensions.ValidateCacheResult_Success(result, CoreTaskCacheStateFlags.NotInitialized, false, this.TestOutputHelper, this.TestCaseLogger);
            result.Result.Should().Be("Success");
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_TimeoutAsync()
        {
            // Failure: No Cache expiration, operation times out in 1 second before task completes in 10 seconds
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<string?> result = await testCoreCachedTask.TestRefreshCacheAsync(() => CoreCacheTestExtensions.TestDelayTask<string?>("Success", this.TestCaseLogger, new TimeSpan(0, 0, 10)), CoreTaskCacheLookupFlags.CurrentCacheLookup, new TimeSpan(0, 0, 1), CancellationToken.None);

            CoreCacheTestExtensions.ValidateCacheResult_Timeout(result, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            result.Result.Should().BeNull();
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_InfiniteAsync()
        {
            // Failure: No Cache expiration, operation times out in 1 second.  Task is blocked.
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<string?> result = await testCoreCachedTask.TestRefreshCacheAsync(() => CoreCacheTestExtensions.TestDelayTask<string?>("Success", this.TestCaseLogger, Timeout.InfiniteTimeSpan), CoreTaskCacheLookupFlags.CurrentCacheLookup, new TimeSpan(0, 0, 1), CancellationToken.None);

            CoreCacheTestExtensions.ValidateCacheResult_Timeout(result, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            result.Result.Should().BeNull();
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_CanceledAsync()
        {
            // Failure: No Cache expiration, operation is cancelled after .1 seconds.
            using var cts = new CancellationTokenSource();
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            cts.CancelAfter(CoreCacheTestExtensions.MinimumCancelDelay);
            ICoreTaskResult<string?> result = await testCoreCachedTask.TestRefreshCacheAsync(() => CoreCacheTestExtensions.TestDelayTask<string?>("Success", this.TestCaseLogger, CoreCacheTestExtensions.Operation30SecTimeout), CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout, cts.Token);

            CoreCacheTestExtensions.ValidateCacheResult_Canceled(result, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);
            result.Result.Should().BeNull();
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_Exception()
        {
            // Failure: No Cache expiration, operation throws exception
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<string?> result = await testCoreCachedTask.TestRefreshCacheAsync(() => throw new ArgumentException(), CoreTaskCacheLookupFlags.CurrentCacheLookup, new TimeSpan(0, 0, 8), CancellationToken.None);

            CoreCacheTestExtensions.ValidateCacheResult_Exception(result, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            result.Result.Should().BeNull();
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_VersionAsync()
        {
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.QuickCacheExpiration, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.Current);
            await CoreCacheTestExtensions.ValidateCacheTask_TestVersionAsync(testCoreCachedTask, CoreCacheTestExtensions.StringKeyValue, CoreTaskCacheStateFlags.Current, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_Expire_Refresh_Clear_RefreshAsync()
        {
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.CacheShortTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);

            await CoreCacheTestExtensions.ValidateCacheTask_TestExpireRefreshClearRefreshAsync(testCoreCachedTask, CoreCacheTestExtensions.StringKeyValue, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_Expire_Refresh_Clear_Refresh_ExpireAsync()
        {
            // Verify Cache Clear
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.CacheShortTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            await CoreCacheTestExtensions.ValidateCacheTask_TestExpireRefreshClearRefreshExpireAsync(testCoreCachedTask, CoreCacheTestExtensions.StringKeyValue, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_Expire_Refresh_Clear_Refresh_RefreshAsync()
        {
            // Initialize a quick expiration cache
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.CacheShortTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);

            await CoreCacheTestExtensions.ValidateCacheTask_TestExpireRefreshClearRefreshRefreshAsync(testCoreCachedTask, CoreCacheTestExtensions.StringKeyValue, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_CurrentCacheAsync()
        {
            var testCoreCachedTask = new TestCoreCachedTask<string?>(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            var result = await CoreCacheTestExtensions.ValidateCacheTask_TestCurrentCacheAsync<string?>(testCoreCachedTask, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            result.Should().NotStartWith("00:00:00.0000");
        }

        [Fact]
        public async Task CachedTaskBaseIntegration_ConcurrentAsync()
        {
            var testCoreCachedTask = new TestCoreCachedTask<string?>(new TimeSpan(0, 0, 0, 0, 300), this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);

            await CoreCacheTestExtensions.ValidateCacheTask_TestConcurrentAsync(testCoreCachedTask, CoreTaskCacheStateFlags.NotInitialized, this.TestCaseLogger);
        }
    }
}
