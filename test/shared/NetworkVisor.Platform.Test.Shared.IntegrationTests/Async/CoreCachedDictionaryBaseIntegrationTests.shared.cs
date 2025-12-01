// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreCachedDictionaryBaseIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Core Cached Dictionary Base Integration Tests.</summary>
// ***********************************************************************

using System.Collections.Concurrent;
using FluentAssertions;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Async
{
    /// <summary>
    /// Class CoreCachedDictionaryBaseIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreCachedDictionaryBaseIntegrationTests))]

    public class CoreCachedDictionaryBaseIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreCachedDictionaryBaseIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreCachedDictionaryBaseIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task CachedDictionaryBaseIntegration_SuccessAsync()
        {
            // Success: No Cache expiration, task completes before operation timeout
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> result = await testCoreCachedTask.TestRefreshDictionaryAsync(() => CoreCacheTestExtensions.TestDelayTask<string, ICoreObjectCacheable?>(TestCoreCachedDictionary.CreateTestDictionary(), this.TestCaseLogger), CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout, CancellationToken.None);

            CoreCacheTestExtensions.ValidateCacheResult_Success(result, CoreTaskCacheStateFlags.NotInitialized, false, this.TestOutputHelper, this.TestCaseLogger);

            result.Result![CoreCacheTestExtensions.KeyFound].Should().NotBeNull().And.Subject.Should().Be(CoreCacheTestExtensions.ValueFoundObject);
            result.Result[CoreCacheTestExtensions.KeyNull].Should().BeNull();
        }

        [Fact]
        public async Task CachedDictionaryBaseIntegration_DefaultTimeoutAsync()
        {
            // Success: No Cache expiration, task completes before operation timeout
            var testCoreCachedTask = new TestCoreCachedDictionary(this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> result = await testCoreCachedTask.TestRefreshDictionaryAsync(() => CoreCacheTestExtensions.TestDelayTask<string, ICoreObjectCacheable?>(TestCoreCachedDictionary.CreateTestDictionary(), this.TestCaseLogger), CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout, CancellationToken.None);

            CoreCacheTestExtensions.ValidateCacheResult_Success(result, CoreTaskCacheStateFlags.NotInitialized, false, this.TestOutputHelper, this.TestCaseLogger);

            result.Result![CoreCacheTestExtensions.KeyFound].Should().NotBeNull().And.Subject.Should().Be(CoreCacheTestExtensions.ValueFoundObject);
            result.Result[CoreCacheTestExtensions.KeyNull].Should().BeNull();
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_TimeoutAsync()
        {
            // Failure: No Cache expiration, operation times out in 1 second before task completes in 5 seconds
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> result = await testCoreCachedTask.TestRefreshDictionaryAsync(() => CoreCacheTestExtensions.TestDelayTask<string, ICoreObjectCacheable?>(TestCoreCachedDictionary.CreateTestDictionary(), this.TestCaseLogger, CoreCacheTestExtensions.CacheMediumTimeout), CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.QuickCacheExpiration, CancellationToken.None);

            CoreCacheTestExtensions.ValidateCacheResult_Timeout(result, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            result.Result.Should().BeNull();
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_InfiniteAsync()
        {
            // Failure: No Cache expiration, operation times out in 1 second.  Task is blocked.
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> result = await testCoreCachedTask.TestRefreshDictionaryAsync(() => CoreCacheTestExtensions.TestDelayTask<string, ICoreObjectCacheable?>(TestCoreCachedDictionary.CreateTestDictionary(), this.TestCaseLogger, Timeout.InfiniteTimeSpan), CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.QuickCacheExpiration, CancellationToken.None);

            CoreCacheTestExtensions.ValidateCacheResult_Timeout(result, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            result.Result.Should().BeNull();
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_Canceled()
        {
            // Failure: No Cache expiration, operation is cancelled after .1 seconds.
            using var cts = new CancellationTokenSource();
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            cts.CancelAfter(CoreCacheTestExtensions.MinimumCancelDelay);
            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> result = await testCoreCachedTask.TestRefreshDictionaryAsync(() => CoreCacheTestExtensions.TestDelayTask<string, ICoreObjectCacheable?>(TestCoreCachedDictionary.CreateTestDictionary(), this.TestCaseLogger, CoreCacheTestExtensions.Operation30SecTimeout), CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout, cts.Token);

            CoreCacheTestExtensions.ValidateCacheResult_Canceled(result, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            result.Result.Should().BeNull();
        }

        [Fact]
        public async Task CoreCachedTaskDictionaryIntegration_ExceptionAsync()
        {
            // Failure: No Cache expiration, operation throws exception
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> result = await testCoreCachedTask.TestRefreshDictionaryAsync(() => throw new ArgumentException(), CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.CacheMediumTimeout, CancellationToken.None);

            CoreCacheTestExtensions.ValidateCacheResult_Exception(result, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            result.Result.Should().BeNull();
        }

        [Theory]
        [InlineData(CoreTaskCacheStateFlags.Current)]
        [InlineData(CoreTaskCacheStateFlags.Error)]
        [InlineData(CoreTaskCacheStateFlags.NotInitialized)]
        [InlineData(CoreTaskCacheStateFlags.UpdateInProgress)]
        public void CachedTaskDictionaryIntegration_InitialState(CoreTaskCacheStateFlags taskCacheStateFlags)
        {
            // Success: No Cache expiration. Initial cache state.
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, taskCacheStateFlags);

            testCoreCachedTask.IsCacheCurrent.Should().Be(taskCacheStateFlags == CoreTaskCacheStateFlags.Current);
            testCoreCachedTask.IsCacheInvalid.Should().Be(taskCacheStateFlags == CoreTaskCacheStateFlags.NotInitialized || taskCacheStateFlags == CoreTaskCacheStateFlags.Error);
            testCoreCachedTask.IsCacheNotInitialized.Should().Be(taskCacheStateFlags == CoreTaskCacheStateFlags.NotInitialized);
            testCoreCachedTask.IsCacheUpdating.Should().Be(taskCacheStateFlags == CoreTaskCacheStateFlags.UpdateInProgress);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_Expire_RefreshAsync()
        {
            // Create a cache that expires in 10000 ticks.  Wait for 10200 ticks
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.QuickCacheExpiration, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.Current);

            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> taskResult = await CoreCacheTestExtensions.ValidateCacheDictionaryResult_ExpireRefreshAsync<ConcurrentDictionary<string, ICoreObjectCacheable?>?>(testCoreCachedTask, CoreTaskCacheStateFlags.Current, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_VersionAsync()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.QuickCacheExpiration, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.Current);

            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> taskResult = await CoreCacheTestExtensions.ValidateCacheDictionaryResult_VersionAsync<ConcurrentDictionary<string, ICoreObjectCacheable?>?>(testCoreCachedTask, CoreTaskCacheStateFlags.Current, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_Clear_Refresh_ExpireAsync()
        {
            // Verify Cache Clear
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.QuickCacheExpiration, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);

            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> taskResult = await CoreCacheTestExtensions.ValidateCacheDictionaryResult_ClearRefreshExpireAsync<ConcurrentDictionary<string, ICoreObjectCacheable?>?>(testCoreCachedTask, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_Clear_RefreshAsync()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);

            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> taskResult = await CoreCacheTestExtensions.ValidateCacheDictionaryResult_ClearRefreshAsync<ConcurrentDictionary<string, ICoreObjectCacheable?>?>(testCoreCachedTask, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_Clear_Refresh_RefreshAsync()
        {
            // Initialize a quick expiration cache
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);

            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> taskResult = await CoreCacheTestExtensions.ValidateCacheDictionaryResult_ClearRefreshRefreshAsync<ConcurrentDictionary<string, ICoreObjectCacheable?>?>(testCoreCachedTask, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_ConcurrentAsync()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.QuickCacheExpiration, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);

            await CoreCacheTestExtensions.ValidateCacheDictionaryResult_ConcurrentAsync<ConcurrentDictionary<string, string?>?>(testCoreCachedTask, CoreTaskCacheStateFlags.NotInitialized, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_RefreshCacheBeforeAsync()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);

            ICoreTaskResult<ConcurrentDictionary<string, ICoreObjectCacheable?>?> taskResult = await CoreCacheTestExtensions.ValidateCacheDictionaryResult_RefreshCacheBeforeAsync<ConcurrentDictionary<string, ICoreObjectCacheable?>?>(testCoreCachedTask, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_UpdateTestCacheForKeyAsync_CacheMiss()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);

            await CoreCacheTestExtensions.ValidateCacheDictionaryResult_Cache_UpdateTestCacheForKeyAsync_CacheMiss<ConcurrentDictionary<string, ICoreObjectCacheable?>?>(testCoreCachedTask, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_UpdateTestCacheForKeyAsync_RefreshOnMiss()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<ICoreObjectCacheable?> taskResult = await testCoreCachedTask.TestRequestKeyValueAsync(() => CoreCacheTestExtensions.TestDelayTask<string, ICoreObjectCacheable?>(TestCoreCachedDictionary.CreateTestDictionary(), this.TestCaseLogger, CoreCacheTestExtensions.MinimumTaskDelay), CoreCacheTestExtensions.KeyMiss, CoreTaskCacheLookupFlags.RefreshCacheOnMissLookup, CoreCacheTestExtensions.Operation30SecTimeout, CancellationToken.None);

            // Caller Key that is not found in cache to trigger refresh

            // Verify initial state of cache
            if (testCoreCachedTask.IsCacheExpired)
            {
                this.TestOutputHelper.WriteLine($"Cache Expired:{Environment.NewLine}{testCoreCachedTask.ToStringWithParentsPropNameMultiLine()}");
                testCoreCachedTask.IsCacheExpired.Should().BeFalse();
            }

            // Confirm adding missed key increases cache version
            testCoreCachedTask.CacheVersion.Should().Be(2);
            testCoreCachedTask.IsCacheCurrent.Should().BeTrue();
            testCoreCachedTask.IsCacheInvalid.Should().BeFalse();
            testCoreCachedTask.IsCacheNotInitialized.Should().BeFalse();
            testCoreCachedTask.IsCacheUpdating.Should().BeFalse();

            taskResult.Should().NotBeNull();
            taskResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();
            taskResult.Result.Should().NotBeNull().And.BeAssignableTo<ICoreObjectCacheable?>();
            taskResult.Result.Should().Be(CoreCacheTestExtensions.ValueMissObject);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_UpdateTestCacheForKeyAsync_NotFound()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            ICoreTaskResult<ICoreObjectCacheable?> taskResult = await testCoreCachedTask.TestRequestKeyValueAsync(() => CoreCacheTestExtensions.TestDelayTask<string, ICoreObjectCacheable?>(TestCoreCachedDictionary.CreateTestDictionary(), this.TestCaseLogger, CoreCacheTestExtensions.MinimumTaskDelay), CoreCacheTestExtensions.KeyNotFound, CoreTaskCacheLookupFlags.Cache, CoreCacheTestExtensions.Operation30SecTimeout, CancellationToken.None);

            // Caller Key that is not found in cache

            // Verify initial state of cache
            if (testCoreCachedTask.IsCacheExpired)
            {
                this.TestOutputHelper.WriteLine($"Cache Expired:{Environment.NewLine}{testCoreCachedTask.ToStringWithParentsPropNameMultiLine()}");
                testCoreCachedTask.IsCacheExpired.Should().BeFalse();
            }

            testCoreCachedTask.CacheVersion.Should().Be(1);
            testCoreCachedTask.IsCacheCurrent.Should().BeTrue();
            testCoreCachedTask.IsCacheInvalid.Should().BeFalse();
            testCoreCachedTask.IsCacheNotInitialized.Should().BeFalse();
            testCoreCachedTask.IsCacheUpdating.Should().BeFalse();

            taskResult.Should().NotBeNull();
            taskResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();
            taskResult.Result.Should().BeNull();
        }

        [Fact]
        public void CachedTaskDictionaryIntegration_TryAddCacheItem()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);

            testCoreCachedTask.CacheVersion.Should().Be(0);
            testCoreCachedTask.TryAddCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueFoundObject).Should().BeTrue();
            testCoreCachedTask.CacheVersion.Should().Be(1);
            testCoreCachedTask.TryGetCacheItem(CoreCacheTestExtensions.KeyFound, out ICoreObjectCacheable? foundValue).Should().BeTrue();

            foundValue.Should().Be(CoreCacheTestExtensions.ValueFoundObject);
            testCoreCachedTask.TryAddCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueFoundObject).Should().BeFalse();
            testCoreCachedTask.CacheVersion.Should().Be(1);
        }

        [Fact]
        public void CachedTaskDictionaryIntegration_TryRemoveCacheItem()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            testCoreCachedTask.CacheVersion.Should().Be(0);
            testCoreCachedTask.TryRemoveCacheItem(CoreCacheTestExtensions.KeyFound, out ICoreObjectCacheable? foundValue).Should().BeFalse();
            testCoreCachedTask.CacheVersion.Should().Be(0);
            foundValue.Should().BeNull();
            testCoreCachedTask.TryAddCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueFoundObject).Should().BeTrue();
            testCoreCachedTask.CacheVersion.Should().Be(1);
            testCoreCachedTask.TryRemoveCacheItem(CoreCacheTestExtensions.KeyFound, out foundValue).Should().BeTrue();
            testCoreCachedTask.CacheVersion.Should().Be(2);
            foundValue.Should().Be(CoreCacheTestExtensions.ValueFoundObject);
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_TryGetCacheItemAsync()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            testCoreCachedTask.CacheVersion.Should().Be(0);
            testCoreCachedTask.TryGetCacheItem(CoreCacheTestExtensions.KeyFound, out ICoreObjectCacheable? foundValue).Should().BeFalse();
            testCoreCachedTask.CacheVersion.Should().Be(0);
            foundValue.Should().BeNull();
            testCoreCachedTask.TryAddCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueFoundObject).Should().BeTrue();
            testCoreCachedTask.CacheVersion.Should().Be(1);
            testCoreCachedTask.TryGetCacheItem(CoreCacheTestExtensions.KeyFound, out foundValue).Should().BeTrue();
            testCoreCachedTask.CacheVersion.Should().Be(1);
            foundValue.Should().Be(CoreCacheTestExtensions.ValueFoundObject);

            await testCoreCachedTask.ClearCacheAsync(testCoreCachedTask.Logger);

            // Cache state after clear
            this.TestOutputHelper.WriteLine($"Cache State after Clear: {testCoreCachedTask.TaskCacheStateFlags}");

            testCoreCachedTask.CacheVersion.Should().Be(2);
            testCoreCachedTask.TryGetCacheItem(CoreCacheTestExtensions.KeyFound, out foundValue).Should().BeFalse();
            testCoreCachedTask.CacheVersion.Should().Be(2);
            foundValue.Should().BeNull();
        }

        [Fact]
        public async Task CachedTaskDictionaryIntegration_UpdateCacheItemAsync()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            testCoreCachedTask.CacheVersion.Should().Be(0);
            testCoreCachedTask.UpdateCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueFoundObject).Should().Be(CoreCacheTestExtensions.ValueFoundObject);
            testCoreCachedTask.CacheVersion.Should().Be(1);

            // Test item already in cache
            testCoreCachedTask.TryAddCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueFoundObject).Should().BeFalse();
            testCoreCachedTask.CacheVersion.Should().Be(1);

            // Update existing item with new value, removes and adds so CacheVersion is +2
            testCoreCachedTask.UpdateCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueFoundObject).Should().Be(CoreCacheTestExtensions.ValueFoundObject);
            testCoreCachedTask.CacheVersion.Should().Be(3);

            await testCoreCachedTask.ClearCacheAsync(testCoreCachedTask.Logger);

            testCoreCachedTask.CacheVersion.Should().Be(4);
            testCoreCachedTask.TryGetCacheItem(CoreCacheTestExtensions.KeyFound, out ICoreObjectCacheable? foundValue).Should().BeFalse();
            testCoreCachedTask.CacheVersion.Should().Be(4);
            foundValue.Should().BeNull();
        }

        [Fact]
        public void CachedTaskDictionaryIntegration_TryUpdateCacheItem()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            testCoreCachedTask.CacheVersion.Should().Be(0);

            // Try to update with empty cache
            testCoreCachedTask.TryUpdateCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueMissObject, CoreCacheTestExtensions.ValueFoundObject).Should().BeFalse();
            testCoreCachedTask.CacheVersion.Should().Be(0);

            // Add item to cache
            testCoreCachedTask.TryAddCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueFoundObject).Should().BeTrue();
            testCoreCachedTask.CacheVersion.Should().Be(1);

            // Replace KeyFound with ValueMiss
            testCoreCachedTask.TryUpdateCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueMissObject, CoreCacheTestExtensions.ValueFoundObject).Should().BeTrue();
            testCoreCachedTask.CacheVersion.Should().Be(2);

            // Validate cache contains ValueMiss
            testCoreCachedTask.TryGetCacheItem(CoreCacheTestExtensions.KeyFound, out ICoreObjectCacheable? foundValue).Should().BeTrue();
            testCoreCachedTask.CacheVersion.Should().Be(2);
            foundValue.Should().Be(CoreCacheTestExtensions.ValueMissObject);
        }

        [Fact]
        public void CachedTaskDictionaryIntegration_FindFirstOrDefault()
        {
            var testCoreCachedTask = new TestCoreCachedDictionary(CoreCacheTestExtensions.CacheNoTimeout, this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheStateFlags.NotInitialized);
            testCoreCachedTask.CacheVersion.Should().Be(0);

            // Try to find with empty cache
            testCoreCachedTask.FindFirstOrDefault((pair) => pair.Value?.Equals(CoreCacheTestExtensions.ValueFoundObject) ?? false).Should().BeNull();
            testCoreCachedTask.CacheVersion.Should().Be(0);

            // Add item to cache
            testCoreCachedTask.TryAddCacheItem(CoreCacheTestExtensions.KeyFound, CoreCacheTestExtensions.ValueFoundObject).Should().BeTrue();
            testCoreCachedTask.CacheVersion.Should().Be(1);

            // Find with item in cache
            testCoreCachedTask.FindFirstOrDefault((pair) => pair.Value?.Equals(CoreCacheTestExtensions.ValueFoundObject) ?? false).Should().Be(CoreCacheTestExtensions.ValueFoundObject);
            testCoreCachedTask.CacheVersion.Should().Be(1);

            // Add item to cache
            testCoreCachedTask.TryAddCacheItem(CoreCacheTestExtensions.KeyNull, null).Should().BeTrue();
            testCoreCachedTask.CacheVersion.Should().Be(2);

            // Find with item key with null value in cache
            testCoreCachedTask.FindFirstOrDefault((pair) => pair.Key?.Equals(CoreCacheTestExtensions.KeyNull) ?? false).Should().BeNull();
        }
    }
}
