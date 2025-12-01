// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreNetworkInterfaceIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Immutable;
using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Connectivity;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Networking.Preferred;
using NetworkVisor.Core.Networking.Sockets.Client;
using NetworkVisor.Core.Networking.Sockets.Listeners;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking
{
    /// <summary>
    /// Class NetworkInterfaceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkInterfaceIntegrationTests))]

    public class CoreNetworkInterfaceIntegrationTests : CoreTestCaseBase
    {
        private const int InvalidPort = 31337;
        private Lazy<ICoreNetworkInterface?> preferredNetworkInterfaceLazy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkInterfaceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkInterfaceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.preferredNetworkInterfaceLazy = new Lazy<ICoreNetworkInterface?>(() => this.TestNetworkServices.PreferredNetwork?.PreferredNetworkInterface);
        }

        public ICoreNetworkInterface? PreferredNetworkInterface => this.preferredNetworkInterfaceLazy.Value;

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
            this.PreferredNetworkInterface.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkInterface>();
        }

        /// <summary>
        /// Defines the test method NetworkInterface_OutputAllNetworkInterfaces.
        /// </summary>
        [Fact]
        public void NetworkInterface_OutputAllNetworkInterfaces()
        {
            foreach (ICoreNetworkInterface? networkInterface in this.TestNetworkingSystem.GetAllNetworkInterfaces())
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method PreferredNetworkInterface_GetScoredInterfaceDnsServerAddresses.
        /// </summary>
        [Fact]
        public void PreferredNetworkInterface_GetScoredInterfaceDnsServerAddresses()
        {
            ICorePreferredNetwork preferredNetwork = this.TestNetworkServices.PreferredNetwork;

            preferredNetwork.Should().NotBeNull();
            preferredNetwork.Should().BeAssignableTo<ICorePreferredNetwork>();

            ICoreNetworkInterface? preferredDnsServerNetworkInterface = preferredNetwork!.PreferredDnsServerNetworkInterface;

            preferredDnsServerNetworkInterface.Should().NotBeNull();
            preferredDnsServerNetworkInterface!.IsValidSystemNetworkInterface.Should().BeTrue();

            ISet<CoreIPAddressScoreResult> scoredDnsServerAddresses = preferredDnsServerNetworkInterface.GetScoredInterfaceDnsServerAddresses(CoreNetworkInterfaceChangeEvent.NetworkInterfaceEnumerationEvent);
            scoredDnsServerAddresses.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Scored Dns Servers: [{string.Join("]\n[", scoredDnsServerAddresses.Select(score => score.ToStringWithPropName()))}]");
        }

        /// <summary>
        /// Defines the test method NetworkInterface.
        /// </summary>
        [Fact]
        public async Task PreferredNetworkInterface_GetInterfaceScoredDhcpServerAddressesAsync()
        {
            ICorePreferredNetwork preferredNetwork = this.TestNetworkServices.PreferredNetwork;

            preferredNetwork.Should().NotBeNull();
            preferredNetwork.Should().BeAssignableTo<ICorePreferredNetwork>();

            ICoreNetworkInterface? preferredDhcpServerNetworkInterface = preferredNetwork!.PreferredDhcpServerNetworkInterface;

            if (preferredDhcpServerNetworkInterface?.PreferredDhcpServerAddress is null)
            {
                this.TestOutputHelper.WriteLine("No network interfaces with an active DHCP server and gateway.");
            }
            else
            {
                preferredDhcpServerNetworkInterface.Should().NotBeNull();
                preferredDhcpServerNetworkInterface!.IsValidSystemNetworkInterface.Should().BeTrue();
                preferredDhcpServerNetworkInterface.PreferredNetworkAddress.Should().NotBeNull();

                ISet<CoreIPAddressScoreResult> dhcpServerAddresses = await preferredDhcpServerNetworkInterface!.GetScoredInterfaceDhcpServerAddressesAsync(CoreNetworkInterfaceChangeEvent.NetworkInterfaceEnumerationEvent);

                this.TestOutputHelper.WriteLine($"PreferredDhcpServerNetworkInterface: {preferredDhcpServerNetworkInterface.DisplayName}");
                this.TestOutputHelper.WriteLine($"Dhcp Servers: {string.Join(", ", dhcpServerAddresses.Select(item => item.ToStringWithParentsPropName()))}");
                this.TestOutputHelper.WriteLine();
                this.TestOutputHelper.WriteLine($"Dhcp Discovered Servers".CenterTitle());

                foreach (CoreIPAddressScoreResult dhcpServer in preferredDhcpServerNetworkInterface.DhcpServerAddressesDiscovered)
                {
                    this.TestOutputHelper.WriteLine(dhcpServer.ToStringWithPropNameMultiLine());
                    this.TestOutputHelper.WriteLine();
                }

                preferredDhcpServerNetworkInterface.PreferredNetworkAddress.Should().NotBeNull();

                if (preferredDhcpServerNetworkInterface.GetIsDhcpEnabled())
                {
                    dhcpServerAddresses.Should().NotHaveCount(0);
                }
                else
                {
                    dhcpServerAddresses.Should().HaveCount(0);
                }
            }
        }

        /// <summary>
        /// Defines the test method NetworkInterface.
        /// </summary>
        [Fact]
        public void PreferredNetworkInterface_GetDiscoveredDhcpServerAddresses()
        {
            ICorePreferredNetwork preferredNetwork = this.TestNetworkServices.PreferredNetwork;

            preferredNetwork.Should().NotBeNull();
            preferredNetwork.Should().BeAssignableTo<ICorePreferredNetwork>();

            ICoreNetworkInterface? preferredDhcpServerNetworkInterface = preferredNetwork!.PreferredDhcpServerNetworkInterface;

            if (preferredDhcpServerNetworkInterface is null)
            {
                this.TestOutputHelper.WriteLine("No network interfaces with an active DHCP server and gateway.");
            }
            else
            {
                preferredDhcpServerNetworkInterface!.IsValidSystemNetworkInterface.Should().BeTrue();

                this.TestOutputHelper.WriteLine($"PreferredDhcpServerNetworkInterface: {preferredDhcpServerNetworkInterface.DisplayName}");
                this.TestOutputHelper.WriteLine();
                this.TestOutputHelper.WriteLine($"Dhcp Discovered Servers".CenterTitle());

                foreach (CoreIPAddressScoreResult dhcpServer in preferredDhcpServerNetworkInterface.DhcpServerAddressesDiscovered)
                {
                    this.TestOutputHelper.WriteLine(dhcpServer.ToStringWithPropNameMultiLine());
                    this.TestOutputHelper.WriteLine();
                }
            }
        }

        /// <summary>
        /// Defines the test method NetworkInterface.
        /// </summary>
        [Fact]
        public void PreferredNetworkInterface_GetScoredGatewayAddresses()
        {
            ICorePreferredNetwork preferredNetwork = this.TestNetworkServices.PreferredNetwork;

            preferredNetwork.Should().NotBeNull();
            preferredNetwork.Should().BeAssignableTo<ICorePreferredNetwork>();

            ICoreNetworkInterface? preferredGatewayNetworkInterface = preferredNetwork!.PreferredGatewayNetworkInterface;

            preferredGatewayNetworkInterface.Should().NotBeNull();
            preferredGatewayNetworkInterface!.IsValidSystemNetworkInterface.Should().BeTrue();
            preferredGatewayNetworkInterface.GetScoredInterfaceGatewayAddresses().Should().NotHaveCount(0);
            this.TestOutputHelper.WriteLine($"Scored Gateways: [{string.Join("]\n[", preferredGatewayNetworkInterface.GetScoredInterfaceGatewayAddresses().Select(score => score.ToStringWithPropName()))}]");

            preferredGatewayNetworkInterface.HasGateway.Should().BeTrue();
            preferredGatewayNetworkInterface.HasActiveGateway.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method PreferredNetworkInterface_ObjectBag_Output.
        /// </summary>
        [Fact]
        public void PreferredNetworkInterface_ObjectBag_Output()
        {
            this.PreferredNetworkInterface.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkInterface>();
            this.TestOutputHelper.WriteLine($"Object Count: {this.PreferredNetworkInterface!.Count}");

            foreach (ICoreObjectItem item in this.PreferredNetworkInterface.FindAllItems())
            {
                this.TestOutputHelper.WriteLine(item.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method NetworkInterface_GetActiveTcpConnectionsOnInterface_Output.
        /// </summary>
        [Fact]
        public void NetworkInterface_GetActiveTcpConnectionsOnInterface_Output()
        {
            this.PreferredNetworkInterface.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"{string.Join("\n\t", this.PreferredNetworkInterface!.GetActiveTcpConnectionsOnInterface().Select(tcpConnection => $"[LocalEndPoint: {tcpConnection.LocalEndPoint}, RemoteEndPoint: {tcpConnection.RemoteEndPoint}, State: {tcpConnection.State}"))}\n");
        }

        /// <summary>
        /// Defines the test method NetworkInterface_GetActiveTcpListenersOnInterface_Output.
        /// </summary>
        [Fact]
        public void NetworkInterface_GetActiveTcpListenersOnInterface_Output()
        {
            this.PreferredNetworkInterface.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"{string.Join("\n\t", this.PreferredNetworkInterface!.GetActiveTcpListenersOnInterface().Select(endpoint => endpoint.ToString()))}\n");
        }

        /// <summary>
        /// Defines the test method NetworkInterface_GetActiveUdpListenersOnInterface_Output.
        /// </summary>
        [Fact]
        public void NetworkInterface_GetActiveUdpListenersOnInterface_Output()
        {
            this.PreferredNetworkInterface.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"{string.Join("\n\t", this.PreferredNetworkInterface!.GetActiveUdpListenersOnInterface().Select(endpoint => endpoint.ToString()))}\n");
        }

        /// <summary>
        /// Defines the test method NetworkInterface_TcpPortInUseOnInterface.
        /// </summary>
        [Fact]
        public void NetworkInterface_TcpPortInUseOnInterface()
        {
            if (!this.TestOperatingSystem.IsIOS && !this.TestOperatingSystem.IsAndroid)
            {
                this.PreferredNetworkInterface.Should().NotBeNull();
                this.PreferredNetworkInterface!.PreferredIPAddress.IsNullNoneAnyOrLoopback().Should().BeFalse();
                using var tcpListener = new CoreTcpListener(new CoreSocketListenerOptions(new CoreIPEndPoint(this.PreferredNetworkInterface.PreferredIPAddress!, 0)), this.TestCaseLogger);
                tcpListener.Start();
                CoreIPEndPoint? localIPEndPoint = tcpListener.ActiveIPEndPoint;
                tcpListener.IsActive.Should().BeTrue();
                localIPEndPoint.Should().NotBeNull();
                this.TestOutputHelper.WriteLine($"TCP Socket Bound to {localIPEndPoint}");
                this.PreferredNetworkInterface!.TcpPortInUseOnInterface(localIPEndPoint!.Port, this.TestCaseLogger).Should().BeTrue();
                tcpListener.Stop();
            }
        }

        /// <summary>
        /// Defines the test method NetworkInterface_TcpPortInUse_BogusPort.
        /// </summary>
        [Fact]
        public void NetworkInterface_TcpPortInUseOnInterface_BogusPort()
        {
            this.PreferredNetworkInterface.Should().NotBeNull();

            this.PreferredNetworkInterface!.TcpPortInUseOnInterface(InvalidPort, this.TestCaseLogger).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method NetworkInterface_UdpPortInUseOnInterface.
        /// </summary>
        [Fact]
        public void NetworkInterface_UdpPortInUseOnInterface()
        {
            if (!this.TestOperatingSystem.IsIOS && !this.TestOperatingSystem.IsAndroid)
            {
                this.PreferredNetworkInterface.Should().NotBeNull();
                this.PreferredNetworkInterface!.PreferredIPAddress.IsNullNoneAnyOrLoopback().Should().BeFalse();

                using var udpClient = new CoreUdpClient(this.TestCaseServiceProvider, new CoreIPEndPoint(this.PreferredNetworkInterface!.PreferredIPAddress!, 0), this.TestCaseLogger);
                var localIPEndPoint = new CoreIPEndPoint(udpClient.ClientSocket.LocalEndPoint as IPEndPoint);
                localIPEndPoint.Should().NotBeNull();
                this.TestOutputHelper.WriteLine($"UDP Socket Bound to {localIPEndPoint}");
                this.PreferredNetworkInterface!.UdpPortInUseOnInterface(localIPEndPoint!.Port, this.TestCaseLogger).Should().BeTrue();
            }
        }

        /// <summary>
        /// Defines the test method NetworkInterface_UdpPortInUseOnInterface_BogusPort.
        /// </summary>
        [Fact]
        public void NetworkInterface_UdpPortInUseOnInterface_BogusPort()
        {
            this.PreferredNetworkInterface.Should().NotBeNull();

            this.PreferredNetworkInterface!.UdpPortInUseOnInterface(InvalidPort, this.TestCaseLogger).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method NetworkInterface_DiscoverDhcpAddressesFromGatewayAsync_Output.
        /// </summary>
        [Fact]
        public async Task NetworkInterface_DiscoverDhcpAddressesFromGatewayAsync_Output()
        {
            foreach (ICoreNetworkInterface networkInterface in this.TestNetworkingSystem.GetAllActiveNetworkInterfaces())
            {
                // Preferred network address will be null when there are no unicast addresses.
                if (networkInterface.PreferredNetworkAddress is null)
                {
                    networkInterface.GetSystemUnicastAddresses().Should().BeEmpty();
                    continue;
                }

                ISet<CoreIPAddressScoreResult> dhcpServers = await networkInterface.DiscoverDhcpAddressesFromGatewayAsync();

                dhcpServers.Should().NotBeNull();

                this.TestOutputHelper.WriteLine($"{networkInterface.DisplayName}: [{string.Join(",", dhcpServers)}]");
            }
        }

        /// <summary>
        /// Defines the test method NetworkInterface_GetGlobalUnicastAddressesAsync_Output.
        /// </summary>
        [Fact]
        public async Task NetworkInterface_GetGlobalUnicastAddressesAsync_Output()
        {
            UnicastIPAddressInformationCollection unicastAddresses = await IPGlobalProperties.GetIPGlobalProperties().GetUnicastAddressesAsync();

            ISet<CoreUnicastIPAddressInfoScoreResult> unicastAddressScores = CoreUnicastIPAddressInfoScoreResult.ScoreUnicastAddresses(unicastAddresses.ToImmutableHashSet(), this.TestOperatingSystem, out _, out _, null);

            foreach (CoreUnicastIPAddressInfoScoreResult unicastIPAddressInfoScoreResult in unicastAddressScores)
            {
                UnicastIPAddressInformation unicastAddress = unicastIPAddressInfoScoreResult.UnicastAddress;
                this.TestOutputHelper.WriteLine($"    Total Score: {(int)unicastIPAddressInfoScoreResult.UnicastIPAddressInfoScore}");
                this.TestOutputHelper.WriteLine($"    Scores: [{unicastIPAddressInfoScoreResult.UnicastIPAddressInfoScore}]");
                this.TestOutputHelper.WriteLine($"    NetworkInterface: {unicastIPAddressInfoScoreResult.NetworkInterface?.DisplayName}");

                this.TestOutputHelper.WriteLine($"    Address: {unicastAddress.Address}");
                this.TestOutputHelper.WriteLine($"    IPv4Mask: {unicastAddress.IPv4Mask}");
                this.TestOutputHelper.WriteLine($"    PrefixLength: {unicastAddress.PrefixLength}");

                if (this.TestOperatingSystem.IsWindowsPlatform)
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    this.TestOutputHelper.WriteLine($"    IsTransient: {unicastAddress.IsTransient}");
                    this.TestOutputHelper.WriteLine($"    IsDnsEligible: {unicastAddress.IsDnsEligible}");
#pragma warning restore CA1416 // Validate platform compatibility
                }

                this.TestOutputHelper.WriteLine();
            }
        }
    }
}
