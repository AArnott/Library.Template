// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 01-13-2025
//
// Last Modified By : SteveBu
// Last Modified On : 01-13-2025
// ***********************************************************************
// <copyright file="CoreNetworkDeviceMonitoringServiceIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>Integration tests for CoreNetworkDeviceMonitoringService</summary>
#if NV_USE_HANGFIRE
using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.Networking.Monitoring;
using NetworkVisor.Core.Networking.Monitoring.Service;
using NetworkVisor.Core.Networking.Ping;
using NetworkVisor.Core.Networking.Services.Ping;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Scheduling.Services;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Paramore.Brighter;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Monitoring.Service
{
    /// <summary>
    /// Integration tests for CoreNetworkDeviceMonitoringService.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkDeviceMonitoringServiceIntegrationTests))]
    public class CoreNetworkDeviceMonitoringServiceIntegrationTests : CoreSchedulingTestCaseBase
    {
        private readonly List<Guid> _createdDeviceIds = [];
        private readonly List<IPAddress> _monitoredIpAddresses = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkDeviceMonitoringServiceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkDeviceMonitoringServiceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void DeviceMonitoringIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void DeviceMonitoringIntegration_ServiceInitialization()
        {
            // Assert - Verify service is properly initialized
            _ = this.TestNetworkDeviceMonitoringService.Should().NotBeNull()
                .And.Subject.Should().BeAssignableTo<ICoreNetworkDeviceMonitoringService>();

            _ = this.TestNetworkDeviceMonitoringService.Should().BeAssignableTo<CoreNetworkDeviceMonitoringService>();

            // Verify initial state
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(0);
            _ = this.TestNetworkDeviceMonitoringService.AvailableDeviceCount.Should().Be(0);
            _ = this.TestNetworkDeviceMonitoringService.UnavailableDeviceCount.Should().Be(0);

            this.TestOutputHelper.WriteLine($"Monitoring service initialized with {this.TestNetworkDeviceMonitoringService.DeviceCount} devices");
        }

        [Fact]
        public void DeviceMonitoringIntegration_ServiceDependencies()
        {
            // Assert - Verify required dependencies are available
            _ = this.TestSchedulingService.Should().NotBeNull()
                .And.Subject.Should().BeAssignableTo<ICoreSchedulingBackgroundService>();

            _ = this.CommandProcessor.Should().NotBeNull()
                .And.Subject.Should().BeAssignableTo<IAmACommandProcessor>();

            _ = this.TestNetworkServices.NetworkPing.Should().NotBeNull()
                .And.Subject.Should().BeAssignableTo<ICoreNetworkPing>();

            this.TestOutputHelper.WriteLine("All required dependencies are properly injected");
        }

        [Fact]
        public void DeviceMonitoringIntegration_AddDevice_ValidParameters_ShouldSucceed()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("192.168.1.100");
            var pingInterval = TimeSpan.FromMinutes(2);
            const int timeout = 3000;
            const int maxRetries = 2;
            const string displayName = "Test Device";
            var metadata = new Dictionary<string, object> { ["TestKey"] = "TestValue" };

            // Act
            bool result = this.TestNetworkDeviceMonitoringService.AddDevice(
                deviceId, ipAddress, pingInterval, timeout, maxRetries, displayName, metadata);

            // Assert
            _ = result.Should().BeTrue("because the device should be added successfully");
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(1);

            CoreNetworkDeviceAvailabilityInfo? deviceInfo = this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress);
            _ = deviceInfo.Should().NotBeNull("because the device was just added");
            _ = deviceInfo!.DeviceID.Should().Be(deviceId);
            _ = deviceInfo.IPAddress.Should().Be(ipAddress);
            _ = deviceInfo.PingInterval.Should().Be(pingInterval);
            _ = deviceInfo.Timeout.Should().Be(timeout);
            _ = deviceInfo.MaxRetries.Should().Be(maxRetries);
            _ = deviceInfo.DisplayName.Should().Be(displayName);
            _ = deviceInfo.Metadata.Should().ContainKey("TestKey").WhoseValue.Should().Be("TestValue");
            _ = deviceInfo.AvailabilityState.Should().Be(CoreNetworkDeviceAvailabilityState.AddedButNotPinged);
            _ = deviceInfo.IsMonitored.Should().BeTrue("because a recurring job should be created");
            _ = deviceInfo.RecurringJobId.Should().NotBeNullOrEmpty();

            // Cleanup
            this._monitoredIpAddresses.Add(ipAddress);
            this.TestOutputHelper.WriteLine($"Successfully added device {ipAddress} with ID {deviceId}");
        }

        [Fact]
        public void DeviceMonitoringIntegration_AddDevice_DefaultParameters_ShouldSucceed()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("10.0.0.50");

            // Act
            bool result = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress);

            // Assert
            _ = result.Should().BeTrue();
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(1);

            CoreNetworkDeviceAvailabilityInfo? deviceInfo = this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress);
            _ = deviceInfo.Should().NotBeNull();
            _ = deviceInfo!.PingInterval.Should().Be(CoreNetworkDeviceMonitoringConstants.DefaultPingInterval);
            _ = deviceInfo.Timeout.Should().Be(CoreNetworkDeviceMonitoringConstants.DefaultPingTimeout);
            _ = deviceInfo.MaxRetries.Should().Be(CoreNetworkDeviceMonitoringConstants.DefaultMaxRetries);
            _ = deviceInfo.DisplayName.Should().BeNull();
            _ = deviceInfo.Metadata.Should().BeEmpty();

            // Cleanup
            this._monitoredIpAddresses.Add(ipAddress);
            this.TestOutputHelper.WriteLine($"Successfully added device {ipAddress} with default parameters");
        }

        [Fact]
        public void DeviceMonitoringIntegration_AddDevice_DuplicateIPAddress_ShouldFail()
        {
            // Arrange
            var deviceId1 = Guid.NewGuid();
            var deviceId2 = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("172.16.0.10");

            // Act
            bool firstResult = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId1, ipAddress);
            bool secondResult = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId2, ipAddress);

            // Assert
            _ = firstResult.Should().BeTrue("because the first device should be added successfully");
            _ = secondResult.Should().BeFalse("because duplicate IP addresses should not be allowed");
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(1, "because only one device should be added");

            // Cleanup
            this._monitoredIpAddresses.Add(ipAddress);
            this.TestOutputHelper.WriteLine($"Correctly rejected duplicate IP address {ipAddress}");
        }

        [Theory]
        [InlineData(null)]
        public void DeviceMonitoringIntegration_AddDevice_NullIPAddress_ShouldThrow(IPAddress? ipAddress)
        {
            // Arrange
            var deviceId = Guid.NewGuid();

            // Act & Assert
            _ = Assert.Throws<ArgumentNullException>(() =>
                this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress!));

            this.TestOutputHelper.WriteLine("Correctly threw exception for null IP address");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void DeviceMonitoringIntegration_AddDevice_InvalidTimeout_ShouldThrow(int invalidTimeout)
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("192.168.1.200");

            // Act & Assert
            _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
                this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress, timeout: invalidTimeout));

            this.TestOutputHelper.WriteLine($"Correctly threw exception for invalid timeout: {invalidTimeout}");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void DeviceMonitoringIntegration_AddDevice_InvalidMaxRetries_ShouldThrow(int invalidMaxRetries)
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("192.168.1.201");

            // Act & Assert
            _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
                this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress, maxRetries: invalidMaxRetries));

            this.TestOutputHelper.WriteLine($"Correctly threw exception for invalid max retries: {invalidMaxRetries}");
        }

        [Fact]
        public void DeviceMonitoringIntegration_RemoveDevice_ExistingDevice_ShouldSucceed()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("192.168.2.100");
            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress);

            // Act
            bool result = this.TestNetworkDeviceMonitoringService.RemoveDevice(ipAddress);

            // Assert
            _ = result.Should().BeTrue("because the device should be removed successfully");
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(0);
            _ = this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress).Should().BeNull();

            this.TestOutputHelper.WriteLine($"Successfully removed device {ipAddress}");
        }

        [Fact]
        public void DeviceMonitoringIntegration_RemoveDevice_NonExistentDevice_ShouldFail()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.3.100");

            // Act
            bool result = this.TestNetworkDeviceMonitoringService.RemoveDevice(ipAddress);

            // Assert
            _ = result.Should().BeFalse("because the device does not exist");

            this.TestOutputHelper.WriteLine($"Correctly returned false for non-existent device {ipAddress}");
        }

        [Fact]
        public void DeviceMonitoringIntegration_RemoveDevice_NullIPAddress_ShouldThrow()
        {
            // Act & Assert
            _ = Assert.Throws<ArgumentNullException>(() => this.TestNetworkDeviceMonitoringService.RemoveDevice(null!));

            this.TestOutputHelper.WriteLine("Correctly threw exception for null IP address");
        }

        [Fact]
        public void DeviceMonitoringIntegration_UpdatePingInterval_ValidParameters_ShouldSucceed()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("192.168.4.100");
            var originalInterval = TimeSpan.FromMinutes(1);
            var newInterval = TimeSpan.FromMinutes(5);

            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress, originalInterval);
            this._monitoredIpAddresses.Add(ipAddress);

            // Act
            bool result = this.TestNetworkDeviceMonitoringService.UpdatePingInterval(ipAddress, newInterval);

            // Assert
            _ = result.Should().BeTrue("because the ping interval should be updated successfully");

            CoreNetworkDeviceAvailabilityInfo? deviceInfo = this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress);
            _ = deviceInfo.Should().NotBeNull();
            _ = deviceInfo!.PingInterval.Should().Be(newInterval);

            this.TestOutputHelper.WriteLine($"Successfully updated ping interval for {ipAddress} to {newInterval}");
        }

        [Fact]
        public void DeviceMonitoringIntegration_UpdatePingInterval_NonExistentDevice_ShouldFail()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.5.100");
            var newInterval = TimeSpan.FromMinutes(3);

            // Act
            bool result = this.TestNetworkDeviceMonitoringService.UpdatePingInterval(ipAddress, newInterval);

            // Assert
            _ = result.Should().BeFalse("because the device does not exist");

            this.TestOutputHelper.WriteLine($"Correctly returned false for non-existent device {ipAddress}");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void DeviceMonitoringIntegration_UpdatePingInterval_InvalidInterval_ShouldThrow(int seconds)
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.6.100");
            var invalidInterval = TimeSpan.FromSeconds(seconds);

            // Act & Assert
            _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
                this.TestNetworkDeviceMonitoringService.UpdatePingInterval(ipAddress, invalidInterval));

            this.TestOutputHelper.WriteLine($"Correctly threw exception for invalid interval: {invalidInterval}");
        }

        [Fact]
        public void DeviceMonitoringIntegration_GetAllMonitoredDevices_ShouldReturnAllDevices()
        {
            // Arrange
            var deviceInfos = new List<(Guid DeviceId, IPAddress DeviceIPAddress)>
            {
                (Guid.NewGuid(), IPAddress.Parse("192.168.7.100")),
                (Guid.NewGuid(), IPAddress.Parse("192.168.7.101")),
                (Guid.NewGuid(), IPAddress.Parse("192.168.7.102")),
            };

            foreach ((Guid deviceId, IPAddress deviceIPAddress) in deviceInfos)
            {
                _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, deviceIPAddress);
                this._monitoredIpAddresses.Add(deviceIPAddress);
            }

            // Act
            IReadOnlyCollection<CoreNetworkDeviceAvailabilityInfo> allDevices = this.TestNetworkDeviceMonitoringService.GetAllMonitoredDevices();

            // Assert
            _ = allDevices.Should().HaveCount(3);
            _ = allDevices.Select(d => d.IPAddress).Should().BeEquivalentTo(deviceInfos.Select(d => d.DeviceIPAddress));

            this.TestOutputHelper.WriteLine($"Successfully retrieved {allDevices.Count} monitored devices");
        }

        [Fact]
        public void DeviceMonitoringIntegration_GetDevicesByState_ShouldFilterCorrectly()
        {
            // Arrange
            var deviceId1 = Guid.NewGuid();
            var deviceId2 = Guid.NewGuid();
            var ipAddress1 = IPAddress.Parse("192.168.8.100");
            var ipAddress2 = IPAddress.Parse("192.168.8.101");

            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId1, ipAddress1);
            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId2, ipAddress2);
            this._monitoredIpAddresses.AddRange([ipAddress1, ipAddress2]);

            // Act
            IReadOnlyCollection<CoreNetworkDeviceAvailabilityInfo> addedDevices =
                this.TestNetworkDeviceMonitoringService.GetDevicesByState(CoreNetworkDeviceAvailabilityState.AddedButNotPinged);

            // Assert
            _ = addedDevices.Should().HaveCount(2);
            _ = addedDevices.Should().OnlyContain(d => d.AvailabilityState == CoreNetworkDeviceAvailabilityState.AddedButNotPinged);

            this.TestOutputHelper.WriteLine($"Found {addedDevices.Count} devices in AddedButNotPinged state");
        }

        [Fact]
        public void DeviceMonitoringIntegration_GetAvailableDevices_InitiallyEmpty()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("192.168.9.100");
            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress);
            this._monitoredIpAddresses.Add(ipAddress);

            // Act
            IReadOnlyCollection<CoreNetworkDeviceAvailabilityInfo> availableDevices = this.TestNetworkDeviceMonitoringService.GetAvailableDevices();

            // Assert
            _ = availableDevices.Should().BeEmpty("because newly added devices are not yet marked as available");
            _ = this.TestNetworkDeviceMonitoringService.AvailableDeviceCount.Should().Be(0);

            this.TestOutputHelper.WriteLine($"Initially found {availableDevices.Count} available devices (expected)");
        }

        [Fact]
        public void DeviceMonitoringIntegration_GetUnavailableDevices_InitiallyEmpty()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("192.168.10.100");
            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress);
            this._monitoredIpAddresses.Add(ipAddress);

            // Act
            IReadOnlyCollection<CoreNetworkDeviceAvailabilityInfo> unavailableDevices = this.TestNetworkDeviceMonitoringService.GetUnavailableDevices();

            // Assert
            _ = unavailableDevices.Should().BeEmpty("because newly added devices are not yet marked as unavailable");
            _ = this.TestNetworkDeviceMonitoringService.UnavailableDeviceCount.Should().Be(0);

            this.TestOutputHelper.WriteLine($"Initially found {unavailableDevices.Count} unavailable devices (expected)");
        }

        [Fact]
        public async Task DeviceMonitoringIntegration_TriggerPing_Loopback_ShouldReturnResult()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.SendToLoopback))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.SendToLoopback} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("127.0.0.1"); // Use localhost for reliable ping
            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress, TimeSpan.FromMinutes(10)); // Long interval to avoid automatic pings
            this._monitoredIpAddresses.Add(ipAddress);

            // Act
            CorePingResult? result = await this.TestNetworkDeviceMonitoringService.TriggerPingAsync(ipAddress);

            // Assert
            _ = result.Should().NotBeNull("because TriggerPingAsync should return a result");
            _ = result!.Address.Should().Be(ipAddress);
            _ = result.Status.Should().BeOneOf(IPStatus.Success, IPStatus.TimedOut); // Localhost should usually respond

            CoreNetworkDeviceAvailabilityInfo? deviceInfo = this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress);
            _ = deviceInfo.Should().NotBeNull();
            _ = deviceInfo!.LastPingResult.Should().NotBeNull("because the ping result should be stored");
            _ = deviceInfo.LastPingResult!.Address.Should().Be(ipAddress);
            _ = deviceInfo.LastPingResult!.Status.Should().Be(result.Status);
            _ = deviceInfo.IsAvailable.Should().BeTrue();
            _ = deviceInfo.IsMonitored.Should().BeTrue();

            this.TestOutputHelper.WriteLine($"Ping result for {ipAddress}: {result.Status}, RTT: {result.RoundtripTime}ms");
        }

        [Fact]
        public async Task DeviceMonitoringIntegration_TriggerPing_PreferredIPAddress_ShouldReturnResult()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            IPAddress? ipAddress = this.TestNetworkingSystem.PreferredLocalNetworkAddress?.IPAddress;
            ipAddress.Should().NotBeNull();

            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress, TimeSpan.FromMinutes(10)); // Long interval to avoid automatic pings
            this._monitoredIpAddresses.Add(ipAddress);

            // Act
            CorePingResult? result = await this.TestNetworkDeviceMonitoringService.TriggerPingAsync(ipAddress);

            // Assert
            _ = result.Should().NotBeNull("because TriggerPingAsync should return a result");
            _ = result!.Address.Should().Be(ipAddress);
            _ = result.Status.Should().BeOneOf(IPStatus.Success, IPStatus.TimedOut); // Localhost should usually respond

            CoreNetworkDeviceAvailabilityInfo? deviceInfo = this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress);
            _ = deviceInfo.Should().NotBeNull();
            _ = deviceInfo!.LastPingResult.Should().NotBeNull("because the ping result should be stored");
            _ = deviceInfo.LastPingResult!.Address.Should().Be(ipAddress);
            _ = deviceInfo.LastPingResult!.Status.Should().Be(result.Status);
            _ = deviceInfo.IsAvailable.Should().BeTrue();
            _ = deviceInfo.IsMonitored.Should().BeTrue();

            this.TestOutputHelper.WriteLine($"Ping result for {ipAddress}: {result.Status}, RTT: {result.RoundtripTime}ms");
        }

        [Fact]
        public async Task DeviceMonitoringIntegration_TriggerPing_NonExistentDevice_ShouldReturnNull()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.11.100");

            // Act
            CorePingResult? result = await this.TestNetworkDeviceMonitoringService.TriggerPingAsync(ipAddress);

            // Assert
            _ = result.Should().BeNull("because the device is not being monitored");

            this.TestOutputHelper.WriteLine($"Correctly returned null for non-monitored device {ipAddress}");
        }

        [Fact]
        public async Task DeviceMonitoringIntegration_TriggerPing_NullIPAddress_ShouldThrow()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => this.TestNetworkDeviceMonitoringService.TriggerPingAsync(null!));

            this.TestOutputHelper.WriteLine("Correctly threw exception for null IP address");
        }

        [Fact]
        public void DeviceMonitoringIntegration_RemoveAllMonitoredDevices_ShouldClearAll()
        {
            // Arrange
            var deviceInfos = new List<(Guid DeviceId, IPAddress DeviceIPAddress)>
            {
                (Guid.NewGuid(), IPAddress.Parse("192.168.12.100")),
                (Guid.NewGuid(), IPAddress.Parse("192.168.12.101")),
                (Guid.NewGuid(), IPAddress.Parse("192.168.12.102")),
            };

            foreach ((Guid deviceId, IPAddress ipAddress) in deviceInfos)
            {
                _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress);
            }

            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(3, "because we added 3 devices");

            // Act
            this.TestNetworkDeviceMonitoringService.RemoveAllMonitoredDevices();

            // Assert
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(0, "because all devices should be removed");
            _ = this.TestNetworkDeviceMonitoringService.GetAllMonitoredDevices().Should().BeEmpty();

            // Verify each device is no longer monitored
            foreach ((_, IPAddress ipAddress) in deviceInfos)
            {
                _ = this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress).Should().BeNull();
            }

            this.TestOutputHelper.WriteLine("Successfully removed all monitored devices");
        }

        [Fact]
        public void DeviceMonitoringIntegration_DeviceCount_ShouldTrackCorrectly()
        {
            // Arrange
            int initialCount = this.TestNetworkDeviceMonitoringService.DeviceCount;

            // Act & Assert - Add devices
            var deviceId1 = Guid.NewGuid();
            var ipAddress1 = IPAddress.Parse("192.168.13.100");
            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId1, ipAddress1);
            this._monitoredIpAddresses.Add(ipAddress1);
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(initialCount + 1);

            var deviceId2 = Guid.NewGuid();
            var ipAddress2 = IPAddress.Parse("192.168.13.101");
            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId2, ipAddress2);
            this._monitoredIpAddresses.Add(ipAddress2);
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(initialCount + 2);

            // Remove devices
            _ = this.TestNetworkDeviceMonitoringService.RemoveDevice(ipAddress1);
            _ = this._monitoredIpAddresses.Remove(ipAddress1);
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(initialCount + 1);

            _ = this.TestNetworkDeviceMonitoringService.RemoveDevice(ipAddress2);
            _ = this._monitoredIpAddresses.Remove(ipAddress2);
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(initialCount);

            this.TestOutputHelper.WriteLine($"Device count correctly tracked: final count = {this.TestNetworkDeviceMonitoringService.DeviceCount}");
        }

        [Fact]
        public void DeviceMonitoringIntegration_DeviceInfo_PropertiesSetCorrectly()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("192.168.14.100");
            var pingInterval = TimeSpan.FromMinutes(3);
            const int timeout = 4000;
            const int maxRetries = 5;
            const string displayName = "Integration Test Device";
            var metadata = new Dictionary<string, object>
            {
                ["Environment"] = "Test",
                ["Priority"] = 1,
                ["Owner"] = "Integration Tests",
            };

            // Act
            _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress, pingInterval, timeout, maxRetries, displayName, metadata);
            this._monitoredIpAddresses.Add(ipAddress);

            // Assert
            CoreNetworkDeviceAvailabilityInfo? deviceInfo = this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress);
            _ = deviceInfo.Should().NotBeNull();

            CoreNetworkDeviceAvailabilityInfo info = deviceInfo!;
            _ = info.DeviceID.Should().Be(deviceId);
            _ = info.IPAddress.Should().Be(ipAddress);
            _ = info.PingInterval.Should().Be(pingInterval);
            _ = info.Timeout.Should().Be(timeout);
            _ = info.MaxRetries.Should().Be(maxRetries);
            _ = info.DisplayName.Should().Be(displayName);
            _ = info.AvailabilityState.Should().Be(CoreNetworkDeviceAvailabilityState.AddedButNotPinged);
            _ = info.RetryCount.Should().Be(0);
            _ = info.AddedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
            _ = info.LastUpdated.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
            _ = info.LastPingResult.Should().BeNull("because no ping has been performed yet");
            _ = info.IsMonitored.Should().BeTrue();
            _ = info.IsAvailable.Should().BeFalse();
            _ = info.IsUnavailable.Should().BeFalse();

            // Verify metadata
            _ = info.Metadata.Should().HaveCount(3);
            _ = info.Metadata["Environment"].Should().Be("Test");
            _ = info.Metadata["Priority"].Should().Be(1);
            _ = info.Metadata["Owner"].Should().Be("Integration Tests");

            this.TestOutputHelper.WriteLine($"Device info properties correctly set for {ipAddress}");
        }

        [Fact]
        public void DeviceMonitoringIntegration_MultipleDevicesWithDifferentStates()
        {
            // Arrange
            var devices = new List<(Guid DeviceId, IPAddress DeviceIPAddress, string DeviceName)>
            {
                (Guid.NewGuid(), IPAddress.Parse("192.168.15.100"), "Device1"),
                (Guid.NewGuid(), IPAddress.Parse("192.168.15.101"), "Device2"),
                (Guid.NewGuid(), IPAddress.Parse("192.168.15.102"), "Device3"),
            };

            // Act - Add all devices
            foreach ((Guid deviceId, IPAddress deviceIPAddress, string deviceName) in devices)
            {
                bool result = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, deviceIPAddress, displayName: deviceName);
                _ = result.Should().BeTrue($"because device {deviceName} should be added successfully");
                this._monitoredIpAddresses.Add(deviceIPAddress);
            }

            // Assert
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(3);

            IReadOnlyCollection<CoreNetworkDeviceAvailabilityInfo> allDevices = this.TestNetworkDeviceMonitoringService.GetAllMonitoredDevices();
            _ = allDevices.Should().HaveCount(3);
            _ = allDevices.Should().OnlyContain(d => d.AvailabilityState == CoreNetworkDeviceAvailabilityState.AddedButNotPinged);

            // Verify each device individually
            foreach ((Guid deviceId, IPAddress ipAddress, string name) in devices)
            {
                CoreNetworkDeviceAvailabilityInfo? deviceInfo = this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress);
                _ = deviceInfo.Should().NotBeNull();
                _ = deviceInfo!.DeviceID.Should().Be(deviceId);
                _ = deviceInfo.DisplayName.Should().Be(name);
                _ = deviceInfo.IsMonitored.Should().BeTrue();
            }

            this.TestOutputHelper.WriteLine($"Successfully managed {devices.Count} devices with different configurations");
        }

        [Fact]
        public void DeviceMonitoringIntegration_ConcurrentAccess_ShouldBeThreadSafe()
        {
            // Arrange
            const int deviceCount = 10;
            var devices = Enumerable.Range(0, deviceCount)
                .Select(i => (Guid.NewGuid(), IPAddress.Parse($"192.168.16.{100 + i}"), $"ConcurrentDevice{i}"))
                .ToList();

            // Act - Add devices concurrently
            _ = Parallel.ForEach(devices, device =>
            {
                (Guid deviceId, IPAddress ipAddress, string name) = device;
                _ = this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress, displayName: name);
            });

            // Collect IP addresses for cleanup
            this._monitoredIpAddresses.AddRange(devices.Select(d => d.Item2));

            // Assert
            _ = this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(deviceCount, "because all devices should be added successfully");

            // Verify all devices are present
            IReadOnlyCollection<CoreNetworkDeviceAvailabilityInfo> allDevices = this.TestNetworkDeviceMonitoringService.GetAllMonitoredDevices();
            _ = allDevices.Should().HaveCount(deviceCount);

            foreach ((Guid deviceId, IPAddress ipAddress, string name) in devices)
            {
                CoreNetworkDeviceAvailabilityInfo? deviceInfo = this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress);
                _ = deviceInfo.Should().NotBeNull($"because device {name} should exist");
                _ = deviceInfo!.DeviceID.Should().Be(deviceId);
                _ = deviceInfo.DisplayName.Should().Be(name);
            }

            this.TestOutputHelper.WriteLine($"Successfully handled concurrent access with {deviceCount} devices");
        }

#if NV_MOCK_CONNECTIVITY
        [Fact]
        public void DeviceMonitoringIntegration_NetworkConnectivityChanged_ShouldRemoveAllDevices()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var ipAddress = IPAddress.Parse("192.168.17.100");
            this.TestNetworkDeviceMonitoringService.AddDevice(deviceId, ipAddress);
            this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(1, "because we added one device");

            // Create a mock network connectivity event
            var mockEvent = new MockNetworkConnectivityEvent(hasNetworkChanged: true);

            // Act
            this.TestNetworkDeviceMonitoringService.NetworkingSystem_NetworkConnectivityChanged(this, mockEvent);

            // Assert
            this.TestNetworkDeviceMonitoringService.DeviceCount.Should().Be(0, "because all devices should be removed on network change");
            this.TestNetworkDeviceMonitoringService.GetDeviceAvailabilityInfo(ipAddress).Should().BeNull();

            this.TestOutputHelper.WriteLine("Successfully removed all devices on network connectivity change");
        }
#endif

        /// <summary>
        /// Releases the resources used by the <see cref="CoreNetworkDeviceMonitoringServiceIntegrationTests"/> class.
        /// </summary>
        /// <param name="disposing">
        /// A boolean value indicating whether the method is called explicitly
        /// (true) or by the garbage collector (false).
        /// </param>
        /// <remarks>
        /// This method ensures that all monitored devices are properly removed
        /// from the test network device monitoring service during cleanup.
        /// Any exceptions encountered during the removal process are logged.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            try
            {
                // Clean up any monitored devices
                foreach (IPAddress ipAddress in this._monitoredIpAddresses.ToList())
                {
                    try
                    {
                        _ = this.TestNetworkDeviceMonitoringService.RemoveDevice(ipAddress);
                    }
                    catch (Exception ex)
                    {
                        this.TestOutputHelper.WriteLine($"Failed to remove device {ipAddress} during cleanup: {ex.Message}");
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

#if NV_MOCK_CONNECTIVITY
        /// <summary>
        /// Mock implementation of ICoreNetworkConnectivityEvent for testing.
        /// </summary>
        private class MockNetworkConnectivityEvent : ICoreNetworkConnectivityEvent
        {
            public MockNetworkConnectivityEvent(bool hasNetworkChanged)
            {
                this.HasNetworkChanged = hasNetworkChanged;
            }

            public bool HasNetworkChanged { get; }
            public bool HasPreferredInterfaceOrNetworkChanged => false;
            public CoreNetworkChangeEvent NetworkChangeEvent => CoreNetworkChangeEvent.NetworkAddressChanged;
            public ICoreNetworkInterface? PreferredNetworkInterface => null;
            public ICoreLocalNetworkServices LocalNetworkServices => null!;
            public ICoreLogger Logger => null!;
            public CancellationToken CancellationToken => CancellationToken.None;
        }
#endif
    }
}
#endif
