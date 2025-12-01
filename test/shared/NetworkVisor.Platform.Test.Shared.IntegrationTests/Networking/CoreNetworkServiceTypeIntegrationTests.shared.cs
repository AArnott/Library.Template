// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreNetworkServiceTypeIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Dhcp.Types;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Ping;
using NetworkVisor.Core.Networking.Services.Arp;
using NetworkVisor.Core.Networking.Services.Sockets.Channel;
using NetworkVisor.Core.Networking.Sockets.Client;
using NetworkVisor.Core.Networking.Sockets.Listeners;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Extensions;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking
{
    /// <summary>
    /// Class CoreNetworkServiceTypeIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkServiceTypeIntegrationTests))]

    public class CoreNetworkServiceTypeIntegrationTests : CoreTestCaseBase
    {
        private static readonly TimeSpan CacheNoTimeout = new(0, 3, 0);  // 3 minutes (test timeout after 5 minutes)
        private static readonly TimeSpan OperationNoTimeout = new(0, 1, 0);     // 1 minute
        private readonly byte[] _sendBuffer = new byte[] { 0, 1, 2, 3 };

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkServiceTypeIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkServiceTypeIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method CoreNetworkServiceTypeIntegrationTests_Ctor.
        /// </summary>
        [Fact]
        public void CoreNetworkServiceTypeIntegrationTests_Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
        }

        /// <summary>
        /// Tests if the ping service is enabled and working.
        /// </summary>
        [Fact]
        public async Task CoreNetworkServiceTypeIntegrationTests_Ping()
        {
            var networkPing = new CoreNetworkPing(this.TestNetworkingSystem, this.TestCaseLogger);

            this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Ping).Should().BeTrue();

            // On Android emulator, ping the gateway address
            if (this.TestOperatingSystem.IsAndroid && this.TestOperatingSystem.DeviceHostType == CoreDeviceHostType.Virtual)
            {
                CorePingResult pingResultAndroid = await networkPing.PingAsync("10.0.2.3");

                this.OutputPingResult(pingResultAndroid);

                pingResultAndroid.Status.Should().Be(IPStatus.Success);
                return;
            }

            // Wait for random delay up to 5 second to prevent denial of service
            this.TestDelay(DateTime.Now.Millisecond % 5000, this.TestCaseLogger).Should().BeTrue();

            // Update to 12 seconds as timeout may occur due to concurrency
            CorePingResult pingResult = await networkPing.PingAsync("dns.google", 15000);

            this.OutputPingResult(pingResult);

            if (CoreAppConstants.IsRunningInCI && pingResult.Status == IPStatus.TimedOut)
            {
                // Cloud clients sometimes cannot ping out
                pingResult.Status.Should().Be(IPStatus.TimedOut);
            }
            else
            {
                pingResult.Status.Should().Be(IPStatus.Success);

                // Make sure we are matching a known Google DNS server
                pingResult.Address.ToString().Should().NotBeNullOrWhiteSpace();
                pingResult.Address.ToString().Should().Match(ip => ip.Equals("8.8.8.8") || ip.Equals("8.8.4.4") || ip.Equals("2001:4860:4860::8844") || ip.Equals("2001:4860:4860::8888"));
            }
        }

        /// <summary>
        /// Tests if the arp service is enabled and working and can get physical address of gateway.
        /// </summary>
        [Fact]
        public async Task CoreNetworkServiceTypeIntegrationTests_ArpGateway()
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
            await ping.PingAsync(gatewayIPAddressSubnet!.IPAddress);

            var networkArpCache = new CoreNetworkArpCache(CacheNoTimeout);

            // Verify initial state of cache
            CoreCacheTestExtensions.ValidateCacheTask_Initial(networkArpCache, CoreTaskCacheStateFlags.NotInitialized, this.TestOutputHelper, this.TestCaseLogger);

            ICoreTaskResult<ICoreNetworkArpDevice?> taskResult = await networkArpCache.RequestArpDeviceAsync(this.TestNetworkServices, gatewayIPAddressSubnet.IPAddress, this.TestCaseLogger, CoreTaskCacheLookupFlags.CurrentCacheLookup, OperationNoTimeout);

            // Verify cache has been updated to version 1
            CoreCacheTestExtensions.ValidateCacheTask_Refresh(networkArpCache, 1, this.TestOutputHelper, this.TestCaseLogger);

            // Verify result did not come from cache.
            CoreCacheTestExtensions.ValidateCacheResult_Success(taskResult, CoreTaskCacheStateFlags.NotInitialized, false, this.TestOutputHelper, this.TestCaseLogger);

            ICoreNetworkArpDevice? arpDevice = taskResult.Result;
            arpDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkArpDevice>();

            this.TestOutputHelper.WriteLine($"ArpDevice: {arpDevice.ToStringWithPropNameMultiLine()}");
            taskResult = await networkArpCache.RequestArpDeviceAsync(this.TestNetworkServices, gatewayIPAddressSubnet.IPAddress, this.TestCaseLogger, CoreTaskCacheLookupFlags.CurrentCacheLookup, OperationNoTimeout);

            // Verify cache has been updated to version 1
            CoreCacheTestExtensions.ValidateCacheTask_Refresh(networkArpCache, 1, this.TestOutputHelper, this.TestCaseLogger);

            // Verify result did not come from cache.
            CoreCacheTestExtensions.ValidateCacheResult_Success(taskResult, CoreTaskCacheStateFlags.NotInitialized, true, this.TestOutputHelper, this.TestCaseLogger);

            arpDevice = taskResult.Result;
            arpDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkArpDevice>();
        }

        /// <summary>
        /// Tests if privileged sockets (less than 1024) are supported.
        /// </summary>
        [Fact]
        public void CoreNetworkServiceTypeIntegrationTests_SocketsPrivileged()
        {
            // Privileged sockets are not supported on Linux, NetCore, and Android due to kernel permissions
            this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.SocketsPrivileged).Should().Be(!(this.TestOperatingSystem.IsLinuxBuildPlatform || this.TestOperatingSystem.IsNetCoreBuildPlatform || this.TestOperatingSystem.IsAndroid));
        }

        /// <summary>
        /// Tests if non privileged sockets (greater than 1023) are supported.
        /// </summary>
        [Fact]
        public void CoreNetworkServiceTypeIntegrationTests_SocketsNonPrivileged()
        {
            int bufferLength = this.TestSendNonPrivilegedPacket(this.TestCaseLogger);

            bufferLength.Should().Be(this._sendBuffer.Length);
        }

        private int TestSendNonPrivilegedPacket(ICoreTestCaseLogger testCaseLogger)
        {
            // Non privileged sockets are supported on all platforms.
            this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.SocketsNonPrivileged).Should().BeTrue();

            CoreUdpListener? udpListener = null;

            IPAddress? preferredIPAddress = this.TestNetworkServices.PreferredLocalNetworkAddress?.IPAddress;

            if (preferredIPAddress is null)
            {
                return 0;
            }

            int bytesSent = 0;

            try
            {
                var coreSocketListenerOptions = new CoreSocketListenerOptions(new CoreIPEndPoint(preferredIPAddress, CoreTestConstants.TestNonPrivilegedUDPPort))
                {
                    Broadcast = true,
                    ProtocolType = ProtocolType.Udp,
                    SocketType = SocketType.Dgram,
                    ReuseAddress = true,
                    SendTimeout = CoreDhcpConstants.DhcpSendTimeout,
                    ReceiveTimeout = CoreDhcpConstants.DhcpReceiveTimeout,
                };

                udpListener = new CoreUdpListener(coreSocketListenerOptions, this.TestCaseLogger);

                udpListener.Connected += this.OnClientConnect;
                udpListener.Disconnected += this.OnClientDisconnect;

                if (udpListener.Start())
                {
                    udpListener.IsActive.Should().BeTrue();
                    testCaseLogger.LogInformation("Started listening on non privileged port {ClientPort}", CoreTestConstants.TestNonPrivilegedUDPPort);

                    bytesSent = CoreUdpSendClient.SendUdpPacket(this.TestCaseServiceProvider, preferredIPAddress, CoreTestConstants.TestNonPrivilegedUDPPort, this._sendBuffer, false, this.TestCaseLogger);

                    testCaseLogger.LogDebug("Success: unprivileged packet sent with length of {BytesSent}", bytesSent);

                    // Wait for 1 second
                    this.TestDelay(1000, this.TestCaseLogger).Should().BeTrue();

                    testCaseLogger.LogDebug("Unprivileged socket waited for 1 second");
                }
                else
                {
                    testCaseLogger.LogDebug("Failed to start listening on non privileged port {ClientPort}", CoreTestConstants.TestNonPrivilegedUDPPort);
                    udpListener.IsActive.Should().BeTrue();
                }
            }
            finally
            {
                if (udpListener is not null)
                {
                    udpListener.Connected -= this.OnClientConnect;
                    udpListener.Disconnected -= this.OnClientDisconnect;
                    udpListener.Stop();
                    udpListener.Dispose();
                    udpListener.IsActive.Should().BeFalse();
                    udpListener = null;

                    testCaseLogger.LogDebug("Unprivileged socket disposed in finally");
                }
            }

            bytesSent.Should().BeGreaterThan(0);

            return bytesSent;
        }

        /// <summary>
        /// Handles the OnClientDisconnect event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="socketErrorEvent">Exception generated when client disconnected.</param>
        private void OnClientDisconnect(object sender, CoreSocketErrorEvent socketErrorEvent)
        {
            if (socketErrorEvent is null)
            {
                throw new ArgumentNullException(nameof(socketErrorEvent));
            }

            socketErrorEvent.Logger.LogDebug(socketErrorEvent.Exception, "Unprivileged client was disconnected");
        }

        /// <summary>
        /// Handles the OnClientConnect event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="channel">ClientSocket socket channel.</param>
        private void OnClientConnect(object sender, CoreSocketChannel channel)
        {
            channel.Should().NotBeNull();

            // channel.IsConnected.Should().BeTrue();
            channel.InputStream.Should().NotBeNull();

            var inputBytes = channel.InputStream.ToByteArray();

            this.TestOutputHelper.WriteLine($"Input bytes: {inputBytes.ToHexString()}");

            // channel.InputStream!.Length.Should().Be(this._sendBuffer.Length);

            // inputBytes.Take((int)channel.InputStream!.Length).Should().BeEquivalentTo(this._sendBuffer);
            channel.Logger.LogDebug(
                "Success: Unprivileged UPD test packet received on {LocalEndPoint} with channel id {ChannelId} was received from {RemoteEndPoint}.",
                channel.Connection.LocalIPEndpoint,
                channel.Connection.Id,
                channel.Connection.RemoteIPEndpoint);

            this.TestOutputHelper.WriteLine(
                "Success: Unprivileged UDP test packet received on {0} with channel id {1} was received from {2}.",
                channel.Connection.LocalIPEndpoint!,
                channel.Connection.Id!,
                channel.Connection.RemoteIPEndpoint!);
        }

        /// <summary>
        /// Outputs the ping reply.
        /// </summary>
        /// <param name="pingResult">The ping result.</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void OutputPingResult(CorePingResult pingResult)
        {
            this.TestOutputHelper.WriteLine($"Ping Status: {pingResult.Status}");
            this.TestOutputHelper.WriteLine($"Ping Error Message: {pingResult.ErrorMessage}");
            this.TestOutputHelper.WriteLine($"Ping Host Address: {pingResult.Address}");
            this.TestOutputHelper.WriteLine($"Ping RoundtripTime (ms): {pingResult.RoundtripTime}");
        }
    }
}
