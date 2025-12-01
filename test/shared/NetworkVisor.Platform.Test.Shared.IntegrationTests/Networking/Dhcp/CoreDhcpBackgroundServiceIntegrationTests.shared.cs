// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreDhcpBackgroundServiceIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Immutable;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Dhcp.Devices;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Networking.Preferred;
using NetworkVisor.Core.Networking.Services.Dhcp;
using NetworkVisor.Core.Networking.Services.Dhcp.BackgroundService;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.Preferred;
using NetworkVisor.Platform.Test.Extensions;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Dhcp
{
    /// <summary>
    /// Class CoreDhcpBackgroundServiceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreDhcpBackgroundServiceIntegrationTests))]

    public class CoreDhcpBackgroundServiceIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreDhcpBackgroundServiceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreDhcpBackgroundServiceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        // Delay for 3 seconds if not in CI
        private int DefaultWait => CoreAppConstants.IsRunningInCI ? 1000 : 3000;

        public static string TestVendorClassId(Type typeTestClass)
        {
            return CoreAssemblyExtensions.BuiltFrameworkDisplayName is null
            ? StringExtensions.NullString
            : $"Network Visor Test ({typeTestClass.GetTraitOperatingSystem()} {CoreAssemblyExtensions.BuiltFrameworkDisplayName})";
        }

        [Fact]
        public void CoreDhcpBackgroundServiceIntegrationTests_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public async Task CoreDhcpBackgroundServiceIntegrationTests_SendDhcpInformFromPreferredIPAddressToNetworkPreferredDhcpServerAsync()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.DhcpClient} is not available on {this.TestClassType.GetTraitOperatingSystem()}");

                return;
            }

            ICoreNetworkDhcpServerInfo? networkDhcpServerInfo = this.TestNetworkingSystem.FindPreferredNetworkDhcpServerInfo();

            if (networkDhcpServerInfo?.PreferredDhcpServerAddress is null)
            {
                this.TestOutputHelper.WriteLine("No network interfaces with an active DHCP server and gateway.");
                return;
            }

            this.TestOutputHelper.WriteLine($"Sending DhcpInform to: \n{networkDhcpServerInfo.ToStringWithParentsPropNameMultiLine()}\n");

            using var dhcpBackgroundService = new CoreDhcpBackgroundService(this.TestCaseServiceProvider, this.TestNetworkServices, this.TestCaseLogger);

            dhcpBackgroundService.OnDhcpDiscoveredDevice += this.OnDhcpDiscoveredTestDevice;

            var cts = new CancellationTokenSource();

            await dhcpBackgroundService.StartAsync(cts.Token);

            dhcpBackgroundService.IsDhcpClientRunning.Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient));
            dhcpBackgroundService.IsDhcpServerRunning.Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer));
            dhcpBackgroundService.IsRunning.Should().BeTrue();
            dhcpBackgroundService.IsDhcpServerEndPointInUse().Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer));
            dhcpBackgroundService.IsDhcpClientEndPointInUse().Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient));

            // Send DhcpInform to preferred network interface
            var bytesSent = await dhcpBackgroundService.SendDhcpInformFromPreferredIPAddressToNetworkPreferredDhcpServerAsync(TestVendorClassId(this.TestClassType));

            bytesSent.Should().BeGreaterThan(0);

            this.TestDelay(this.DefaultWait, this.TestCaseLogger, cts.Token);

            dhcpBackgroundService.OnDhcpDiscoveredDevice -= this.OnDhcpDiscoveredTestDevice;

            dhcpBackgroundService.Stop();

            dhcpBackgroundService.IsDhcpClientRunning.Should().BeFalse();
            dhcpBackgroundService.IsDhcpServerRunning.Should().BeFalse();
            dhcpBackgroundService.IsRunning.Should().BeFalse();

            dhcpBackgroundService.IsDhcpServerEndPointInUse().Should().BeFalse();
            dhcpBackgroundService.IsDhcpClientEndPointInUse().Should().BeFalse();
        }

        [Fact]
        public async Task CoreDhcpBackgroundServiceIntegrationTests_SendDhcpInformFromPreferredIPAddressToLoopbackAsync()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.DhcpServer} is not available on {this.TestClassType.GetTraitOperatingSystem()}");

                return;
            }

            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.SendToLoopback))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.SendToLoopback} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            using var dhcpBackgroundService = new CoreDhcpBackgroundService(this.TestCaseServiceProvider, this.TestNetworkServices, this.TestCaseLogger);
            dhcpBackgroundService.OnDhcpDiscoveredDevice += this.OnDhcpDiscoveredTestDevice;

            var cts = new CancellationTokenSource();

            await dhcpBackgroundService.StartAsync(cts.Token);
            dhcpBackgroundService.IsDhcpClientRunning.Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient));
            dhcpBackgroundService.IsDhcpServerRunning.Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer));
            dhcpBackgroundService.IsRunning.Should().BeTrue();
            dhcpBackgroundService.IsDhcpServerEndPointInUse().Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer));
            dhcpBackgroundService.IsDhcpClientEndPointInUse().Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient));

            // Send DhcpInform to Loopback
            var bytesSent = await dhcpBackgroundService.SendDhcpInformFromPreferredIPAddressToLoopbackAsync(TestVendorClassId(this.TestClassType));

            bytesSent.Should().BeGreaterThan(0);

            this.TestDelay(this.DefaultWait, this.TestCaseLogger, cts.Token);

            dhcpBackgroundService.OnDhcpDiscoveredDevice -= this.OnDhcpDiscoveredTestDevice;

            dhcpBackgroundService.Stop();

            dhcpBackgroundService.IsDhcpClientRunning.Should().BeFalse();
            dhcpBackgroundService.IsDhcpServerRunning.Should().BeFalse();
            dhcpBackgroundService.IsRunning.Should().BeFalse();

            dhcpBackgroundService.IsDhcpServerEndPointInUse().Should().BeFalse();
            dhcpBackgroundService.IsDhcpClientEndPointInUse().Should().BeFalse();
        }

        [Fact]
        public async Task CoreDhcpBackgroundServiceIntegrationTests_BroadcastDhcpDiscoverFromClientIPAddressSubnet()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.DhcpClient} is not available on {this.TestClassType.GetTraitOperatingSystem()}");

                return;
            }

            ICorePreferredNetwork preferredNetwork = this.TestNetworkServices.PreferredNetwork;
            preferredNetwork.Should().NotBeNull();

            using var dhcpBackgroundService = new CoreDhcpBackgroundService(this.TestCaseServiceProvider, this.TestNetworkServices, this.TestCaseLogger);

            dhcpBackgroundService.OnDhcpDiscoveredDevice += this.OnDhcpDiscoveredTestDeviceDhcpServer;

            var cts = new CancellationTokenSource();

            await dhcpBackgroundService.StartAsync(cts.Token);

            dhcpBackgroundService.IsDhcpClientRunning.Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient));
            dhcpBackgroundService.IsDhcpServerRunning.Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer));
            dhcpBackgroundService.IsRunning.Should().BeTrue();
            dhcpBackgroundService.IsDhcpServerEndPointInUse().Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer));
            dhcpBackgroundService.IsDhcpClientEndPointInUse().Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient));

            string? deviceHostName = this.TestNetworkingSystem.DeviceHostName;

            if (deviceHostName.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                deviceHostName = null;
            }

            // Dhcp Discover broadcasts so we don't need a valid dhcp server address
            preferredNetwork.PreferredNetworkInterface?.PreferredIPAddressSubnet.Should().NotBeNull();

            var bytesSent = dhcpBackgroundService.BroadcastDhcpDiscoverFromClientIPAddressSubnet(
                preferredNetwork.PreferredNetworkInterface?.PreferredIPAddressSubnet!,
                preferredNetwork.PreferredNetworkInterface?.PhysicalAddress!,
                TestVendorClassId(this.TestClassType),
                deviceHostName);

            bytesSent.Should().BeGreaterThan(0);

            this.TestDelay(this.DefaultWait, this.TestCaseLogger, cts.Token);

            dhcpBackgroundService.OnDhcpDiscoveredDevice -= this.OnDhcpDiscoveredTestDeviceDhcpServer;

            dhcpBackgroundService.Stop();

            dhcpBackgroundService.IsDhcpClientRunning.Should().BeFalse();
            dhcpBackgroundService.IsDhcpServerRunning.Should().BeFalse();
            dhcpBackgroundService.IsRunning.Should().BeFalse();

            dhcpBackgroundService.IsDhcpServerEndPointInUse().Should().BeFalse();
            dhcpBackgroundService.IsDhcpClientEndPointInUse().Should().BeFalse();

            this.TestOutputHelper.WriteLine("Discovered Dhcp Servers".CenterTitle());

            ISet<CoreIPAddressScoreResult> dhcpServers = dhcpBackgroundService.DhcpDiscoveredServers;
            dhcpServers.Should().NotBeNull();

            // CI has only static IP addresses so this will fail.
            // CoreDhcpBackgroundService.DiscoveredDhcpServers.Count.Should().BeGreaterThan(0);
            foreach (CoreIPAddressScoreResult dhcpServerScore in dhcpServers)
            {
                this.TestOutputHelper.WriteLine(dhcpServerScore.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task CoreDhcpBackgroundServiceIntegrationTests_SendDhcpRequestFromPreferredIPAddressToLoopbackAsync()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.DhcpServer} is not available on {this.TestClassType.GetTraitOperatingSystem()}");

                return;
            }

            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.SendToLoopback))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.SendToLoopback} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            using var dhcpBackgroundService = new CoreDhcpBackgroundService(this.TestCaseServiceProvider, this.TestNetworkServices, this.TestCaseLogger);

            dhcpBackgroundService.OnDhcpDiscoveredDevice += this.OnDhcpDiscoveredTestDevice;

            var cts = new CancellationTokenSource();

            await dhcpBackgroundService.StartAsync(cts.Token);

            dhcpBackgroundService.IsDhcpClientRunning.Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient));
            dhcpBackgroundService.IsDhcpServerRunning.Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer));
            dhcpBackgroundService.IsRunning.Should().BeTrue();
            dhcpBackgroundService.IsDhcpServerEndPointInUse().Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer));
            dhcpBackgroundService.IsDhcpClientEndPointInUse().Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient));

            ICorePreferredNetwork preferredNetwork = this.TestNetworkServices.PreferredNetwork;

            preferredNetwork.Should().NotBeNull();

            // Send Caller to Loopback
            var bytesSent = await dhcpBackgroundService.SendDhcpRequestFromPreferredIPAddressToLoopbackAsync(TestVendorClassId(this.TestClassType));

            bytesSent.Should().BeGreaterThan(0);

            // Delay for DefaultWait
            this.TestDelay(this.DefaultWait, this.TestCaseLogger, cts.Token);

            dhcpBackgroundService.OnDhcpDiscoveredDevice -= this.OnDhcpDiscoveredTestDevice;

            dhcpBackgroundService.Stop();
            dhcpBackgroundService.IsDhcpClientRunning.Should().BeFalse();
            dhcpBackgroundService.IsDhcpServerRunning.Should().BeFalse();
            dhcpBackgroundService.IsRunning.Should().BeFalse();

            dhcpBackgroundService.IsDhcpServerEndPointInUse().Should().BeFalse();
            dhcpBackgroundService.IsDhcpClientEndPointInUse().Should().BeFalse();
        }

        [Fact]
        public async Task DhcpDeviceCacheIntegration_SendDhcpInformToNetworkPreferredDhcpServer_Output()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.DhcpServer} is not available on {this.TestClassType.GetTraitOperatingSystem()}");

                return;
            }

            ICoreNetworkDhcpServerInfo? networkDhcpServerInfo = this.TestNetworkingSystem.FindPreferredNetworkDhcpServerInfo();

            if (networkDhcpServerInfo is null)
            {
                this.TestOutputHelper.WriteLine("No network interfaces with an active DHCP server and gateway.");
                return;
            }

            this.TestOutputHelper.WriteLine($"Sending DhcpInform to: \n{networkDhcpServerInfo.ToStringWithParentsPropNameMultiLine()}\n");

            using CancellationTokenSource cts = new();
            using ICoreDhcpBackgroundService dhcpBackgroundService = new CoreDhcpBackgroundService(this.TestCaseServiceProvider, this.TestNetworkServices, this.TestCaseLogger);

            using ICoreDhcpDeviceCache dhcpDeviceCache = new CoreDhcpDeviceCache(dhcpBackgroundService, CoreTaskCacheStateFlags.NotInitialized);

            dhcpBackgroundService.IsRunning.Should().BeFalse();

            // Start DhcpServer after Cache is created.
            await dhcpBackgroundService.StartAsync(cts.Token);

            dhcpBackgroundService.IsDhcpClientRunning.Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient));
            dhcpBackgroundService.IsDhcpServerRunning.Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer));
            dhcpBackgroundService.IsRunning.Should().BeTrue();
            dhcpBackgroundService.IsDhcpServerEndPointInUse().Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer));
            dhcpBackgroundService.IsDhcpClientEndPointInUse().Should().Be(this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient));

            var cacheVersion = dhcpDeviceCache.CacheVersion;

            ICoreTaskResult<ICoreDhcpDiscoveredDevice?>? taskResult = null;

            try
            {
                // Cancel after 15 seconds
                cts.CancelAfter(TimeSpan.FromSeconds(15));

                taskResult = await dhcpDeviceCache.SendDhcpInformToNetworkPreferredDhcpServer(this.TestNetworkServices, this.TestCaseLogger, CoreTaskCacheLookupFlags.CurrentCacheLookup, null, cts.Token);

                taskResult.Should().NotBeNull();

                if (taskResult.IsCompletedSuccessfully)
                {
                    taskResult.Result.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreDhcpDiscoveredDevice?>();
                }
                else if (cts.IsCancellationRequested)
                {
                    taskResult.IsTimedOut.Should().BeTrue();
                    taskResult.Result.Should().BeNull();
                }

                if (taskResult.Result is not null)
                {
                    this.TestOutputHelper.WriteLine($"Dhcp Discovered Device:\n{CoreDhcpDiscoveredDevice.Output(taskResult.Result!, LogLevel.Trace)}");
                    this.TestOutputHelper.WriteLine();
                }
            }
            catch (TaskCanceledException)
            {
                this.TestCaseLogger.LogDebug("SendDhcpInformToLocalDeviceAsync completed with time out.");
            }
            finally
            {
                this.TestOutputHelper.WriteLine($"\n***** Output Dhcp Cache since version {cacheVersion} *****");

                var dhcpCacheItems = dhcpDeviceCache.FindItemsUpdatedSince(cacheVersion).ToList();

                // Verify item returned by SendDhcpInformToNetworkPreferredDhcpServer is in the cache
                if (taskResult?.Result is not null)
                {
                    taskResult.Result.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreDhcpDiscoveredDevice?>();
                    dhcpCacheItems.FirstOrDefault(item => item?.Equals(taskResult.Result) ?? false).Should().NotBeNull();
                }

                int count = 1;
                foreach (ICoreDhcpDiscoveredDevice? item in dhcpCacheItems)
                {
                    item.Should().NotBeNull();

                    this.TestOutputHelper.WriteLine($"DhcpDiscoveredDevice ({count})\n{CoreDhcpDiscoveredDevice.Output(item!, LogLevel.Trace)}");
                    this.TestOutputHelper.WriteLine();

                    try
                    {
                        string json = JsonSerializer.Serialize(item, CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.Formatted, this.TestCaseServiceProvider));
                        this.TestOutputHelper.WriteLine($"DhcpDiscoveredDevice ({count}) JSON:\n{json}");
                    }
                    catch (Exception ex)
                    {
                        this.TestOutputHelper.WriteLine($"Error outputting DhcpDiscoveredDevice ({count}): {ex.Message}");
                    }

                    count++;
                }

                dhcpBackgroundService.Stop();

                try
                {
                    dhcpBackgroundService.IsDhcpClientRunning.Should().BeFalse();
                    dhcpBackgroundService.IsDhcpServerRunning.Should().BeFalse();
                    dhcpBackgroundService.IsRunning.Should().BeFalse();

                    dhcpBackgroundService.IsDhcpClientEndPointInUse().Should().BeFalse();
                    dhcpBackgroundService.IsDhcpServerEndPointInUse().Should().BeFalse();
                }
                catch (Xunit.Sdk.XunitException)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Called when [DHCP discovered device].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        private void OnDhcpDiscoveredTestDeviceDhcpServer(object? sender, CoreDhcpDiscoveredDeviceEvent args)
        {
            ICoreDhcpDiscoveredDevice? coreDhcpDiscoveredDevice = args.CoreDhcpDiscoveredDevice;

            if (coreDhcpDiscoveredDevice is not null)
            {
                this.TestOutputHelper.WriteLine(CoreDhcpDiscoveredDevice.Output(coreDhcpDiscoveredDevice, LogLevel.Trace, "BackgroundService: DhcpDiscoveredDevice"));
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Called when [DHCP discovered device].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        private void OnDhcpDiscoveredTestDevice(object? sender, CoreDhcpDiscoveredDeviceEvent args)
        {
            ICoreDhcpDiscoveredDevice? coreDhcpDiscoveredDevice = args.CoreDhcpDiscoveredDevice;

            if (coreDhcpDiscoveredDevice is not null)
            {
                this.TestOutputHelper.WriteLine(CoreDhcpDiscoveredDevice.Output(coreDhcpDiscoveredDevice, LogLevel.Trace, "BackgroundService: DhcpDiscoveredDevice"));
                this.TestOutputHelper.WriteLine();
            }
        }
    }
}
