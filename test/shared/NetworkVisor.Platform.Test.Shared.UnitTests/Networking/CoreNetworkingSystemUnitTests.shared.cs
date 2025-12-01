// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreNetworkingSystemUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Networking.WiFi;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking
{
    /// <summary>
    /// Class CoreNetworkingSystemUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkingSystemUnitTests))]

    public class CoreNetworkingSystemUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkingSystemUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkingSystemUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void NetworkingSystem_Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.ProcessRunner.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreProcessRunner>();
            this.TestNetworkingSystem.OperationRunner.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreOperationRunner>();
            this.TestNetworkingSystem.Logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
            this.TestNetworkingSystem.WiFiNetworkManager.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreWiFiNetworkManager>();
        }

        /// <summary>
        /// Defines the test method NetworkingSystem_IsServiceSupported.
        /// </summary>
        /// <param name="networkServiceType">Type of the network service.</param>
        /// <param name="isSupported">if set to <see langword="true"/> [is supported].</param>
        [Theory]
        [InlineData(CoreNetworkServiceTypes.None, true)]
#if NV_PLAT_ANDROID
        [InlineData(CoreNetworkServiceTypes.SocketsPrivileged, false)]
        [InlineData(CoreNetworkServiceTypes.SocketsNonPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.Ping, true)]

        // TODO: No access to /proc/net on physical devices
        [InlineData(CoreNetworkServiceTypes.Arp, false)]
        [InlineData(CoreNetworkServiceTypes.DhcpServer, false)] // Privileged sockets not allowed
        [InlineData(CoreNetworkServiceTypes.DhcpClient, false)] // Privileged sockets not allowed
        [InlineData(CoreNetworkServiceTypes.Wmi, false)]

        // TODO: Figure out how to get Physical address for local device and gateway
        [InlineData(CoreNetworkServiceTypes.GatewayPhysicalAddress, false)]
        [InlineData(CoreNetworkServiceTypes.LocalPhysicalAddress, false)]

        // UPnP and MulticastDns work in 6.0.302
        [InlineData(CoreNetworkServiceTypes.UPnP, true)]
        [InlineData(CoreNetworkServiceTypes.Multicast, true)]
        [InlineData(CoreNetworkServiceTypes.MulticastDns, true)]
        [InlineData(CoreNetworkServiceTypes.RunProcess, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadOnly, false)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadWrite, false)]
        [InlineData(CoreNetworkServiceTypes.SendToLoopback, true)]
        [InlineData(CoreNetworkServiceTypes.NativeBonjour, false)]

#elif NV_PLAT_IOS
        [InlineData(CoreNetworkServiceTypes.SocketsPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.SocketsNonPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.Ping, true)]
        [InlineData(CoreNetworkServiceTypes.Arp, false)]
        [InlineData(CoreNetworkServiceTypes.DhcpServer, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpClient, false)]
        [InlineData(CoreNetworkServiceTypes.Wmi, false)]
        [InlineData(CoreNetworkServiceTypes.GatewayPhysicalAddress, false)]
        [InlineData(CoreNetworkServiceTypes.UPnP, true)]
        [InlineData(CoreNetworkServiceTypes.Multicast, true)]
        [InlineData(CoreNetworkServiceTypes.MulticastDns, true)]
        [InlineData(CoreNetworkServiceTypes.LocalPhysicalAddress, false)]
        [InlineData(CoreNetworkServiceTypes.RunProcess, false)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadOnly, false)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadWrite, false)]
        [InlineData(CoreNetworkServiceTypes.SendToLoopback, true)]
        [InlineData(CoreNetworkServiceTypes.NativeBonjour, false)]

#elif NV_PLAT_MACCATALYST
        [InlineData(CoreNetworkServiceTypes.SocketsPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.SocketsNonPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.Ping, true)]
        [InlineData(CoreNetworkServiceTypes.Arp, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpServer, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpClient, false)]
        [InlineData(CoreNetworkServiceTypes.Wmi, false)]
        [InlineData(CoreNetworkServiceTypes.GatewayPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.UPnP, true)]
        [InlineData(CoreNetworkServiceTypes.Multicast, true)]
        [InlineData(CoreNetworkServiceTypes.MulticastDns, true)]
        [InlineData(CoreNetworkServiceTypes.LocalPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.RunProcess, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadOnly, false)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadWrite, false)]
        [InlineData(CoreNetworkServiceTypes.SendToLoopback, true)]
        [InlineData(CoreNetworkServiceTypes.NativeBonjour, true)]

#elif NV_PLAT_MACOS
        [InlineData(CoreNetworkServiceTypes.SocketsPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.SocketsNonPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.Ping, true)]
        [InlineData(CoreNetworkServiceTypes.Arp, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpServer, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpClient, false)]
        [InlineData(CoreNetworkServiceTypes.Wmi, false)]
        [InlineData(CoreNetworkServiceTypes.GatewayPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.UPnP, true)]
        [InlineData(CoreNetworkServiceTypes.Multicast, true)]
        [InlineData(CoreNetworkServiceTypes.MulticastDns, true)]
        [InlineData(CoreNetworkServiceTypes.LocalPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.RunProcess, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadOnly, false)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadWrite, false)]
        [InlineData(CoreNetworkServiceTypes.SendToLoopback, true)]
        [InlineData(CoreNetworkServiceTypes.NativeBonjour, true)]

#elif NET472_OR_GREATER
        [InlineData(CoreNetworkServiceTypes.SocketsPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.SocketsNonPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.Ping, true)]
        [InlineData(CoreNetworkServiceTypes.Arp, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpServer, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpClient, true)]
        [InlineData(CoreNetworkServiceTypes.Wmi, true)]
        [InlineData(CoreNetworkServiceTypes.GatewayPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.UPnP, true)]
        [InlineData(CoreNetworkServiceTypes.Multicast, true)]
        [InlineData(CoreNetworkServiceTypes.MulticastDns, true)]
        [InlineData(CoreNetworkServiceTypes.LocalPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.RunProcess, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadOnly, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadWrite, true)]
        [InlineData(CoreNetworkServiceTypes.SendToLoopback, true)]
        [InlineData(CoreNetworkServiceTypes.NativeBonjour, false)]

#elif NV_PLAT_WPF
        [InlineData(CoreNetworkServiceTypes.SocketsPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.SocketsNonPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.Ping, true)]
        [InlineData(CoreNetworkServiceTypes.Arp, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpServer, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpClient, true)]
        [InlineData(CoreNetworkServiceTypes.Wmi, true)]
        [InlineData(CoreNetworkServiceTypes.GatewayPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.UPnP, true)]
        [InlineData(CoreNetworkServiceTypes.Multicast, true)]
        [InlineData(CoreNetworkServiceTypes.MulticastDns, true)]
        [InlineData(CoreNetworkServiceTypes.LocalPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.RunProcess, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadOnly, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadWrite, true)]
        [InlineData(CoreNetworkServiceTypes.SendToLoopback, true)]
        [InlineData(CoreNetworkServiceTypes.NativeBonjour, false)]

#elif NV_PLAT_WINUI
        [InlineData(CoreNetworkServiceTypes.SocketsPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.SocketsNonPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.Ping, true)]
        [InlineData(CoreNetworkServiceTypes.Arp, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpServer, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpClient, true)]
        [InlineData(CoreNetworkServiceTypes.Wmi, true)]
        [InlineData(CoreNetworkServiceTypes.GatewayPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.UPnP, true)]
        [InlineData(CoreNetworkServiceTypes.Multicast, true)]
        [InlineData(CoreNetworkServiceTypes.MulticastDns, true)]
        [InlineData(CoreNetworkServiceTypes.LocalPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.RunProcess, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadOnly, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadWrite, true)]
        [InlineData(CoreNetworkServiceTypes.SendToLoopback, true)]
        [InlineData(CoreNetworkServiceTypes.NativeBonjour, false)]

#elif NV_PLAT_WINDOWS
        [InlineData(CoreNetworkServiceTypes.SocketsPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.SocketsNonPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.Ping, true)]
        [InlineData(CoreNetworkServiceTypes.Arp, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpServer, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpClient, true)]
        [InlineData(CoreNetworkServiceTypes.Wmi, true)]
        [InlineData(CoreNetworkServiceTypes.GatewayPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.UPnP, true)]
        [InlineData(CoreNetworkServiceTypes.Multicast, true)]
        [InlineData(CoreNetworkServiceTypes.MulticastDns, true)]
        [InlineData(CoreNetworkServiceTypes.LocalPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.RunProcess, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadOnly, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadWrite, true)]
        [InlineData(CoreNetworkServiceTypes.SendToLoopback, true)]
        [InlineData(CoreNetworkServiceTypes.NativeBonjour, false)]

#elif NV_PLAT_LINUX || NV_PLAT_NETCORE
        // Linux
        [InlineData(CoreNetworkServiceTypes.SocketsPrivileged, false)]
        [InlineData(CoreNetworkServiceTypes.SocketsNonPrivileged, true)]
        [InlineData(CoreNetworkServiceTypes.Ping, true)]
        [InlineData(CoreNetworkServiceTypes.Arp, true)]
        [InlineData(CoreNetworkServiceTypes.DhcpServer, false)]
        [InlineData(CoreNetworkServiceTypes.DhcpClient, false)]
        [InlineData(CoreNetworkServiceTypes.Wmi, false)]
        [InlineData(CoreNetworkServiceTypes.GatewayPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.UPnP, true)]
        [InlineData(CoreNetworkServiceTypes.Multicast, true)]
        [InlineData(CoreNetworkServiceTypes.MulticastDns, true)]
        [InlineData(CoreNetworkServiceTypes.LocalPhysicalAddress, true)]
        [InlineData(CoreNetworkServiceTypes.RunProcess, true)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadOnly, false)]
        [InlineData(CoreNetworkServiceTypes.LocalFirewallReadWrite, false)]
        [InlineData(CoreNetworkServiceTypes.SendToLoopback, true)]
        [InlineData(CoreNetworkServiceTypes.NativeBonjour, false)]

#else
#error Framework is undedfined
#endif
#if NV_USE_HANGFIRE || NV_USE_HANGFIRE_MESSAGING
        [InlineData(CoreNetworkServiceTypes.HangfireScheduler, true)]
#else
        [InlineData(CoreNetworkServiceTypes.HangfireScheduler, false)]
#endif
        public void NetworkingSystem_IsServiceSupported(CoreNetworkServiceTypes networkServiceType, bool isSupported)
        {
            // LocalFirewallReadWrite requires elevated permissions
            if (networkServiceType == CoreNetworkServiceTypes.LocalFirewallReadWrite && isSupported && !this.TestOperatingSystem.IsProcessElevated)
            {
                isSupported = false;
            }

            // Starting with IOS and MacCatalyst 18, Send to Local Host is not supported.
            if (networkServiceType == CoreNetworkServiceTypes.SendToLoopback && isSupported)
            {
                if (this.TestOperatingSystem.IsMacCatalystVersionAtLeast(18, 0)
                    || this.TestOperatingSystem.IsIOSVersionAtLeast(18, 0))
                {
                    isSupported = false;
                }
            }

            // MulticastDns and Multicast are only support on macOS is LocalNetworkPolicy is enabled.
            if (this.TestOperatingSystem.IsMacOS && (networkServiceType == CoreNetworkServiceTypes.Multicast || networkServiceType == CoreNetworkServiceTypes.MulticastDns || networkServiceType == CoreNetworkServiceTypes.UPnP))
            {
                isSupported = !this.TestNetworkingSystem.PreferredMulticastNetworkInterface?.IsLocalNetworkAccessRestricted ?? false;
            }

            // NativeBonjour is only available on MacOS
            // TODO: Re-enable when we have a way to test this on MacOS
            if (networkServiceType == CoreNetworkServiceTypes.NativeBonjour && isSupported)
            {
                isSupported = false;
            }

            this.TestNetworkingSystem.IsServiceSupported(networkServiceType).Should().Be(isSupported);

            switch (networkServiceType)
            {
                case CoreNetworkServiceTypes.SocketsPrivileged:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeTrue();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.SocketsNonPrivileged:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeTrue();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.Ping:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeTrue();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.Arp:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeTrue();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.DhcpServer:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeTrue();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.DhcpClient:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeTrue();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.Wmi:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeTrue();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.GatewayPhysicalAddress:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeTrue();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.UPnP:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeTrue();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.Multicast:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeTrue();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.MulticastDns:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeTrue();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.LocalPhysicalAddress:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeTrue();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.RunProcess:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeTrue();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.LocalFirewallReadOnly:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeTrue();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.LocalFirewallReadWrite:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeTrue();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.SendToLoopback:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeTrue();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.NativeBonjour:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeTrue();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
                case CoreNetworkServiceTypes.HangfireScheduler:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeTrue();
                    break;
                case CoreNetworkServiceTypes.None:
                    networkServiceType.IsPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsNonPrivilegedSocketsSupported().Should().BeFalse();
                    networkServiceType.IsPingSupported().Should().BeFalse();
                    networkServiceType.IsArpSupported().Should().BeFalse();
                    networkServiceType.IsDhcpServerSupported().Should().BeFalse();
                    networkServiceType.IsDhcpClientSupported().Should().BeFalse();
                    networkServiceType.IsWmiSupported().Should().BeFalse();
                    networkServiceType.IsGatewayPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsUPnPSupported().Should().BeFalse();
                    networkServiceType.IsMulticastSupported().Should().BeFalse();
                    networkServiceType.IsMulticastDnsSupported().Should().BeFalse();
                    networkServiceType.IsLocalPhysicalAddressSupported().Should().BeFalse();
                    networkServiceType.IsRunProcessSupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadOnlySupported().Should().BeFalse();
                    networkServiceType.IsLocalFirewallReadWriteSupported().Should().BeFalse();
                    networkServiceType.IsSendToLoopbackSupported().Should().BeFalse();
                    networkServiceType.IsNativeBonjourSupported().Should().BeFalse();
                    networkServiceType.IsHangfireSchedulerSupported().Should().BeFalse();
                    break;
            }
        }
    }
}
