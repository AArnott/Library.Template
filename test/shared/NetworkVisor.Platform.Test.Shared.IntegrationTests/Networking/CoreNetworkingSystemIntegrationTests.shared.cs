// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreNetworkingSystemIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Immutable;
using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Networking.Sockets.Client;
using NetworkVisor.Core.Networking.Sockets.Listeners;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Networking.WiFi;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking
{
    /// <summary>
    /// Class CoreNetworkingSystemIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkingSystemIntegrationTests))]

    public class CoreNetworkingSystemIntegrationTests : CoreTestCaseBase
    {
        private const int InvalidPort = 31337;
        private readonly Lazy<ICoreNetworkInterface?> preferredNetworkInterfaceLazy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkingSystemIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkingSystemIntegrationTests(CoreTestClassFixture testClassFixture)
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
        }

        /// <summary>
        /// Defines the test method NetworkingSystem_PreferredLocalIPAddress.
        /// </summary>
        [Fact]
        public void NetworkingSystem_PreferredLocalIPAddress()
        {
            CoreIPAddressSubnet? localAddressSubnet = this.TestNetworkServices.PreferredLocalNetworkAddress?.IPAddressSubnet;

            localAddressSubnet.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"Preferred Local Address: {localAddressSubnet}");
            localAddressSubnet.Should().NotBeNull().And.Subject.Should().NotBe(CoreIPAddressSubnet.None);
        }

        /// <summary>
        /// Defines the test method NetworkingSystem_PreferredLocalIPAddressInfo.
        /// </summary>
        [Fact]
        public void NetworkingSystem_PreferredLocalIPAddressInfo()
        {
            ICoreNetworkAddressInfo? localAddressInfo = this.TestNetworkServices.PreferredLocalNetworkAddress;

            this.TestOutputHelper.WriteLine($"Preferred Local Address Info: {localAddressInfo.ToStringWithPropNameMultiLine()}");

            localAddressInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkAddressInfo>();
            localAddressInfo!.IPAddressSubnet.IsNullOrNone().Should().BeFalse();

            this.TestNetworkServices.PreferredLocalNetworkAddress.Should().NotBeNull();

            if (!this.TestNetworkServices.PreferredLocalNetworkAddress!.PreferredNetworkInterface.IsCellularConnection)
            {
                localAddressInfo.SubnetMask.IsNullOrNone().Should().BeFalse();
            }

            localAddressInfo.IPAddress.IsNullOrNone().Should().BeFalse();

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalPhysicalAddress))
            {
                localAddressInfo.PhysicalAddress.IsNullOrNone().Should().BeFalse();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalPhysicalAddress} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystem_PreferredIPAddressFromConnection.
        /// </summary>
        [Fact]
        public async Task NetworkingSystem_PreferredIPAddressFromConnection()
        {
            ICoreTaskResult<IPAddress?> taskResult = await this.TestNetworkingSystem.PreferredLocalIPAddressFromConnectionAsync();

            taskResult.Should().NotBeNull();
            taskResult.Result.Should().NotBeNull();
            taskResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();
            IPAddress? ipAddress = taskResult.Result;

            this.TestOutputHelper.WriteLine($"IP Address: {ipAddress}");

            ipAddress.Should().NotBeNull().And.Subject.Should().NotBe(IPAddress.None);
        }

        /// <summary>
        /// Defines the test method NetworkingSystem_GetScoredActiveDnsServers_Output.
        /// </summary>
        [Fact]
        public void NetworkingSystem_GetScoredActiveDnsServers_Output()
        {
            ISet<CoreIPAddressScoreResult> dnsServers = this.TestNetworkingSystem.GetGlobalScoredActiveDnsServers();
            dnsServers.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Global Scored Active Dns Servers:\n[{string.Join("]\n[", dnsServers.Select(score => $"{score.ToStringWithPropName()}"))}]");
        }

        /// <summary>
        /// Defines the test method NetworkingSystem_GetGlobalActiveDnsServers_Output.
        /// </summary>
        [Fact]
        public void NetworkingSystem_GetGlobalActiveDnsServers_Output()
        {
            IList<IPAddress> dnsServers = this.TestNetworkingSystem.GetGlobalActiveDnsServers().ToList();
            dnsServers.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Global Active Dns Servers:\n[{string.Join("]\n[", dnsServers)}]");
        }

        /// <summary>
        /// Defines the test method NetworkingSystem_TcpIPEndPointInUseOnInterface.
        /// </summary>
        [Fact]
        public void NetworkingSystem_TcpIPEndPointInUseOnInterface()
        {
            if (!this.TestOperatingSystem.IsIOS)
            {
                this.PreferredNetworkInterface.Should().NotBeNull();
                this.PreferredNetworkInterface!.PreferredIPAddress.IsNullNoneAnyOrLoopback().Should().BeFalse();

                using var tcpListener = new CoreTcpListener(new CoreSocketListenerOptions(new CoreIPEndPoint(this.PreferredNetworkInterface!.PreferredIPAddress!, 0)), this.TestCaseLogger);
                tcpListener.Start();
                CoreIPEndPoint? localEndPoint = tcpListener.ActiveIPEndPoint;
                tcpListener.IsActive.Should().BeTrue();
                localEndPoint.Should().NotBeNull();
                this.TestOutputHelper.WriteLine($"TCP Socket Bound to {localEndPoint}");
                this.TestNetworkingSystem.GetIPEndPointInUse(localEndPoint!, true, this.TestCaseLogger).Should().BeTrue();
                tcpListener.Stop();
            }
        }

        /// <summary>
        /// Defines the test method NetworkInterface_TcpIPEndPointInUse_BogusPort.
        /// </summary>
        [Fact]
        public void NetworkingSystem_TcpIPEndPointInUse_BogusPort()
        {
            this.PreferredNetworkInterface.Should().NotBeNull();
            this.PreferredNetworkInterface!.PreferredIPAddress.IsNullNoneAnyOrLoopback().Should().BeFalse();

            this.TestNetworkingSystem.GetIPEndPointInUse(new CoreIPEndPoint(this.PreferredNetworkInterface!.PreferredIPAddress!, InvalidPort), true, this.TestCaseLogger).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method NetworkingSystem_UdpIPEndPointInUse.
        /// </summary>
        [Fact]
        public void NetworkingSystem_UdpIPEndPointInUse()
        {
            if (!this.TestOperatingSystem.IsIOS)
            {
                this.PreferredNetworkInterface.Should().NotBeNull();
                this.PreferredNetworkInterface!.PreferredIPAddress.IsNullNoneAnyOrLoopback().Should().BeFalse();

                using var udpClient = new CoreUdpClient(this.TestCaseServiceProvider, new CoreIPEndPoint(this.PreferredNetworkInterface!.PreferredIPAddress!, 0), this.TestCaseLogger);
                var localEndPoint = new CoreIPEndPoint(udpClient.ClientSocket.LocalEndPoint as IPEndPoint);
                localEndPoint.Should().NotBeNull();
                this.TestOutputHelper.WriteLine($"UDP Socket Bound to {localEndPoint}");
                this.TestNetworkingSystem.GetIPEndPointInUse(localEndPoint!, false, this.TestCaseLogger).Should().BeTrue();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystem_UdpIPEndPointInUse_BogusPort.
        /// </summary>
        [Fact]
        public void NetworkingSystem_UdpIPEndPointInUse_BogusPort()
        {
            this.PreferredNetworkInterface.Should().NotBeNull();
            this.PreferredNetworkInterface!.PreferredIPAddress.IsNullNoneAnyOrLoopback().Should().BeFalse();

            this.TestNetworkingSystem.GetIPEndPointInUse(new CoreIPEndPoint(this.PreferredNetworkInterface!.PreferredIPAddress!, InvalidPort), false, this.TestCaseLogger).Should().BeFalse();
        }

        [Fact]
        public void NetworkingSystem_WiFiNetworkManager()
        {
            this.TestNetworkingSystem.WiFiNetworkManager.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreWiFiNetworkManager>();
            this.TestNetworkingSystem.WiFiNetworkManager.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);
        }

        [Fact]
        public async Task NetworkingSystem_GetPreferredWiFiNetworkAsync()
        {
            if (!this.TestNetworkingSystem.WiFiNetworkManager.IsWiFiEnabled)
            {
                this.TestOutputHelper.WriteLine("No Wi-Fi adapters are active to test.");
                (await this.TestNetworkingSystem.GetPreferredWiFiNetworkAsync(false)).Should().BeNull();

                return;
            }

            ICoreWiFiNetwork? preferredWiFiNetworkAsync = await this.TestNetworkingSystem.GetPreferredWiFiNetworkAsync(false);
            preferredWiFiNetworkAsync.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreWiFiNetwork>();
        }
    }
}
