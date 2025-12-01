// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreNetworkArpCacheIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Core Network Arp Cache Integration Tests.</summary>
// ***********************************************************************

using System.Collections.Concurrent;
using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Ping;
using NetworkVisor.Core.Networking.Services.Arp;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Arp
{
    /// <summary>
    /// Class CoreNetworkArpCacheIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreNetworkArpCacheIntegrationTests))]

    public class CoreNetworkArpCacheIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkArpCacheIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkArpCacheIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task CoreNetworkArpCacheIntegration_RequestArpDevicesAsync_Refresh_Cache()
        {
            // Make sure Arp is supported on this device
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Arp))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Arp} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var networkArpCache = new CoreNetworkArpCache(CoreCacheTestExtensions.CacheNoTimeout);

            // Verify initial state of cache
            CoreCacheTestExtensions.ValidateCacheTask_Initial(networkArpCache, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            // Refresh the cache to version 1 with a 30-second operation timeout
            ICoreTaskResult<ConcurrentDictionary<IPAddress, ICoreNetworkArpDevice?>?> taskResult = await networkArpCache.RequestArpDevicesAsync(this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout);

            // Verify cache has been updated to version 1
            CoreCacheTestExtensions.ValidateCacheTask_RefreshResult(taskResult, networkArpCache, CoreTaskCacheStateFlags.Current, false, 1, this.TestOutputHelper, this.TestCaseLogger);

            // Caller from the current cache with a 30-second timeout
            taskResult = await networkArpCache.RequestArpDevicesAsync(this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout);

            // Verify result DID come from cache and version is still 1.
            CoreCacheTestExtensions.ValidateCacheResult_Success(taskResult, CoreTaskCacheStateFlags.NotInitialized, true, this.TestOutputHelper, this.TestCaseLogger);
            networkArpCache.CacheVersion.Should().Be(1);
        }

        [Fact]
        public async Task CoreNetworkArpCacheIntegration_RequestArpDevicesAsync_Refresh_Expire_Refresh()
        {
            // Make sure Arp is supported on this device
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Arp))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Arp} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var networkArpCache = new CoreNetworkArpCache(CoreCacheTestExtensions.CacheExtraLongTimeout);

            // Verify initial state of cache
            CoreCacheTestExtensions.ValidateCacheTask_Initial(networkArpCache, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            // Refresh the cache to version 1 with a 30-second operation timeout
            ICoreTaskResult<ConcurrentDictionary<IPAddress, ICoreNetworkArpDevice?>?> taskResult = await networkArpCache.RequestArpDevicesAsync(this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout);

            // Verify cache has been updated to version 1
            CoreCacheTestExtensions.ValidateCacheTask_RefreshResult(taskResult, networkArpCache, CoreTaskCacheStateFlags.Current, false, 1, this.TestOutputHelper, this.TestCaseLogger);

            // Wait for cache to expire
            await CoreCacheTestExtensions.ValidateCacheTask_Operation_WaitExpiredAsync(networkArpCache, 1, this.TestOutputHelper, this.TestCaseLogger);

            // Caller from the cache
            taskResult = await networkArpCache.RequestArpDevicesAsync(this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation15SecTimeout);

            // Verify cache has been updated to version 2
            CoreCacheTestExtensions.ValidateCacheTask_RefreshResult(taskResult, networkArpCache, CoreTaskCacheStateFlags.Current, false, 2, this.TestOutputHelper, this.TestCaseLogger);

            // Verify result did not come from cache.
            networkArpCache.CacheVersion.Should().Be(2);
        }

        [Fact]
        public async Task CoreNetworkArpCacheIntegration_RequestKeyValueAsync_GatewayAddress_Success()
        {
            // Make sure Arp is supported on this device
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Arp))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Arp} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            CoreIPAddressSubnet? gatewayIPAddressSubnet = this.TestNetworkServices.PreferredNetworkGatewayInfo?.GatewayIPAddressSubnet;

            this.TestOutputHelper.WriteLine($"Validating arp cache for gateway at {gatewayIPAddressSubnet}");

            // Verify we have a gateway address
            gatewayIPAddressSubnet.Should().NotBeNull().And.Subject.Should().BeAssignableTo<CoreIPAddressSubnet>();
            gatewayIPAddressSubnet!.IPAddress.Should().NotBeNull();
            gatewayIPAddressSubnet.IPAddress.Should().NotBe(IPAddress.None);

            // We cannot guarantee the localIPAddress is in the device's arp cache.  Ping it to attempt to force it into the cache.
            var ping = new CoreNetworkPing(this.TestNetworkingSystem, this.TestCaseLogger);
            await ping.PingAsync(gatewayIPAddressSubnet.IPAddress);

            CoreNetworkArpCache networkArpCache = new CoreNetworkArpCache(CoreCacheTestExtensions.CacheNoTimeout);

            // Verify initial state of cache
            CoreCacheTestExtensions.ValidateCacheTask_Initial(networkArpCache, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            // Refresh the cache to version 1
            ICoreTaskResult<ICoreNetworkArpDevice?> taskResult = await networkArpCache.RequestArpDeviceAsync(this.TestNetworkServices, gatewayIPAddressSubnet.IPAddress, this.TestCaseLogger, CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout);

            // Verify cache has been updated to version 1
            CoreCacheTestExtensions.ValidateCacheTask_Refresh(networkArpCache, 1, this.TestOutputHelper, this.TestCaseLogger);

            // Verify result did not come from cache.
            CoreCacheTestExtensions.ValidateCacheResult_Success(taskResult, CoreTaskCacheStateFlags.NotInitialized, false, this.TestOutputHelper, this.TestCaseLogger);

            ICoreNetworkArpDevice? arpDevice = taskResult.Result;
            arpDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkArpDevice>();

            this.TestOutputHelper.WriteLine($"ArpDevice: {arpDevice.ToStringWithPropNameMultiLine()}");
        }

        [Fact]
        public async Task CoreNetworkArpCacheIntegration_RequestKeyValueAsync_CacheMiss_CurrentCache()
        {
            // Make sure Arp is supported on this device
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Arp))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Arp} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var networkArpCache = new CoreNetworkArpCache(CoreCacheTestExtensions.CacheNoTimeout);

            // Verify initial state of cache
            CoreCacheTestExtensions.ValidateCacheTask_Initial(networkArpCache, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            // Refresh the cache to version 1
            ICoreTaskResult<ICoreNetworkArpDevice?> taskResult = await networkArpCache.RequestArpDeviceAsync(this.TestNetworkServices, IPAddress.Parse(CoreIPAddressExtensions.StringNonRoutable), this.TestCaseLogger, CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout);

            // Verify cache has been updated to version 1
            CoreCacheTestExtensions.ValidateCacheTask_Refresh(networkArpCache, 1, this.TestOutputHelper, this.TestCaseLogger);

            // Verify result did not come from cache.
            CoreCacheTestExtensions.ValidateCacheResult_Success(taskResult, CoreTaskCacheStateFlags.NotInitialized, false, this.TestOutputHelper, this.TestCaseLogger);

            // Validate a null result and not from cache.
            taskResult.Result.Should().BeNull();
            taskResult.IsFromCache.Should().BeFalse();
        }

        [Fact]
        public async Task CoreNetworkArpCacheIntegration_RequestKeyValueAsync_CacheMiss_RefreshCacheOnMiss()
        {
            // Make sure Arp is supported on this device
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Arp))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Arp} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var networkArpCache = new CoreNetworkArpCache(CoreCacheTestExtensions.CacheNoTimeout);

            // Verify initial state of cache
            CoreCacheTestExtensions.ValidateCacheTask_Initial(networkArpCache, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            // Refresh the cache to version 1.  Refresh on cache miss.
            ICoreTaskResult<ICoreNetworkArpDevice?> taskResult = await networkArpCache.RequestArpDeviceAsync(this.TestNetworkServices, IPAddress.Parse(CoreIPAddressExtensions.StringNonRoutable), this.TestCaseLogger, CoreTaskCacheLookupFlags.RefreshCacheOnMissLookup, CoreCacheTestExtensions.Operation30SecTimeout);

            // Verify cache has been updated to version to 2 after Cache miss.
            CoreCacheTestExtensions.ValidateCacheTask_Refresh(networkArpCache, 2, this.TestOutputHelper, this.TestCaseLogger);

            // Verify result did not come from cache.
            CoreCacheTestExtensions.ValidateCacheResult_Success(taskResult, CoreTaskCacheStateFlags.NotInitialized, false, this.TestOutputHelper, this.TestCaseLogger);

            // Validate a null result and not from cache.
            taskResult.Result.Should().BeNull();
            taskResult.IsFromCache.Should().BeFalse();
        }

        [Fact]
        public async Task CoreNetworkArpCacheIntegration_RequestKeyValueAsync_CacheMiss_ExternalIP()
        {
            // Make sure Arp is supported on this device
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Arp))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Arp} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var networkArpCache = new CoreNetworkArpCache(CoreCacheTestExtensions.CacheNoTimeout);

            // Verify initial state of cache
            CoreCacheTestExtensions.ValidateCacheTask_Initial(networkArpCache, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            // Refresh the cache to version 1.  Refresh on cache miss.
            ICoreTaskResult<ICoreNetworkArpDevice?> taskResult = await networkArpCache.RequestArpDeviceAsync(this.TestNetworkServices, CoreIPAddressExtensions.NetworkVisorComAddress, this.TestCaseLogger, CoreTaskCacheLookupFlags.RefreshCacheOnMissLookup, CoreCacheTestExtensions.Operation30SecTimeout);

            // Verify cache has been updated to version to 2 after Cache miss.
            CoreCacheTestExtensions.ValidateCacheTask_Refresh(networkArpCache, 2, this.TestOutputHelper, this.TestCaseLogger);

            // Verify result did not come from cache.
            CoreCacheTestExtensions.ValidateCacheResult_Success(taskResult, CoreTaskCacheStateFlags.NotInitialized, false, this.TestOutputHelper, this.TestCaseLogger);

            taskResult.Result.Should().BeNull();
            taskResult.IsFromCache.Should().BeFalse();
        }

        [Fact]
        public async Task CoreNetworkArpCacheIntegration_RequestArpDevicesAsync_Output()
        {
            // Make sure Arp is supported on this device
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Arp))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Arp} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var networkArpCache = new CoreNetworkArpCache(CoreCacheTestExtensions.CacheNoTimeout);

            // Verify initial state of cache
            CoreCacheTestExtensions.ValidateCacheTask_Initial(networkArpCache, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            // Refresh the cache to version 1.
            ICoreTaskResult<ConcurrentDictionary<IPAddress, ICoreNetworkArpDevice?>?> taskResult = await networkArpCache.RequestArpDevicesAsync(this.TestNetworkingSystem, this.TestCaseLogger, CoreTaskCacheLookupFlags.CurrentCacheLookup, CoreCacheTestExtensions.Operation30SecTimeout);

            // Verify cache has been updated to version to 1.
            CoreCacheTestExtensions.ValidateCacheTask_RefreshResult(taskResult, networkArpCache, CoreTaskCacheStateFlags.NotInitialized, false, 1, this.TestOutputHelper, this.TestCaseLogger);

            foreach (KeyValuePair<IPAddress, ICoreNetworkArpDevice?> pair in taskResult.Result!.OrderBy(item => item.Value!.IPAddress.ToLong()))
            {
                this.TestOutputHelper.WriteLine($"{pair.Value?.IPAddress.ToString().PadWithDelim(":", 18)}{pair.Value?.PhysicalAddress.ToColonString()}");
            }
        }
    }
}
