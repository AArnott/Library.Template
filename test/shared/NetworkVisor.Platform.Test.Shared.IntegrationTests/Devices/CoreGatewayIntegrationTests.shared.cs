// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests.Devices
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreGatewayIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Networking.Collection;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Devices
{
    /// <summary>
    /// Class CoreGatewayIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreGatewayIntegrationTests))]

    public class CoreGatewayIntegrationTests : CoreTestCaseBase
    {
        private readonly Lazy<ICoreNetworkGatewayInfo> _networkGatewayInfoLazy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreGatewayIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreGatewayIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._networkGatewayInfoLazy = new Lazy<ICoreNetworkGatewayInfo>(this.CreateNetworkGatewayInfo);
        }

        public ICoreNetworkGatewayInfo NetworkGatewayInfo => this._networkGatewayInfoLazy.Value;

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_Ctor.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_Ctor()
        {
            this.NetworkGatewayInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkGatewayInfo>();
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_GatewayNetworkAddressInfo.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_GatewayNetworkAddressInfo()
        {
            this.NetworkGatewayInfo.GatewayNetworkAddressInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkAddressInfo>();
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_PreferredNetworkInterface.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_PreferredNetworkInterface()
        {
            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkInterface>();
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_PreferredNetworkInterface_MatchingSubnetMask.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_PreferredNetworkInterface_MatchingSubnetMask()
        {
            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface.Should().NotBeNull();
            this.NetworkGatewayInfo.GatewayNetworkAddressInfo.Should().NotBeNull();
            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.PreferredIPAddressSubnet.Should().NotBeNull();
            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.PreferredIPAddressSubnet!.SubnetMask.Should().Be(this.NetworkGatewayInfo.GatewayNetworkAddressInfo!.SubnetMask);
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_PreferredNetworkInterface_MatchingSubnetMask.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_PreferredNetworkInterface_CanGatewayRouteIPAddressSubnet()
        {
            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface.Should().NotBeNull();
            this.NetworkGatewayInfo.GatewayNetworkAddressInfo.Should().NotBeNull();
            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.PreferredIPAddressSubnet.Should().NotBeNull();
            this.NetworkGatewayInfo.GatewayNetworkAddressInfo!.IPAddressSubnet.CanGatewayRouteIPAddress(this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.PreferredIPAddress).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_GatewayIPAddressSubnet.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_GatewayIPAddressSubnet()
        {
            CoreIPAddressSubnet? gatewayIPAddressSubnet = this.NetworkGatewayInfo.GatewayIPAddressSubnet;

            this.TestOutputHelper.WriteLine($"GatewayIPAddressSubnet: {gatewayIPAddressSubnet}");
            gatewayIPAddressSubnet.IsNullOrNone().Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_GatewayPhysicalAddressAsync.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_GatewayPhysicalAddressAsync()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.GatewayPhysicalAddress))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.GatewayPhysicalAddress} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                return;
            }

            PhysicalAddress? gatewayPhysicalAddress = this.NetworkGatewayInfo.GatewayPhysicalAddress;

            this.TestOutputHelper.WriteLine($"GatewayNetworkAddressInfo : {this.NetworkGatewayInfo.GatewayNetworkAddressInfo?.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine)}");
            this.TestOutputHelper.WriteLine($"GatewayPhysicalAddress : {gatewayPhysicalAddress.ToDashString()}");

            gatewayPhysicalAddress.Should().NotBeNull().And.Subject.Should().NotBe(PhysicalAddress.None);
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_ToString.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_ToString()
        {
            this.TestOutputHelper.WriteLine($"Network Gateway : {this.NetworkGatewayInfo.ToStringWithPropNameMultiLine()}");
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_ToString_GatewayIPAddressSubnet.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_ToString_GatewayIPAddressSubnet()
        {
            this.NetworkGatewayInfo.GatewayIPAddressSubnet.IsNullOrNone().Should().BeFalse();
            this.TestOutputHelper.WriteLine($"Network Gateway IPAddressSubnet: {this.NetworkGatewayInfo.GatewayIPAddressSubnet}");
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_GatewaySubnetMask_ToString.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_GatewaySubnetMask_ToString()
        {
            if (!this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.IsCellularConnection)
            {
                this.NetworkGatewayInfo.GatewaySubnetMask.Should().NotBeNull().And.Subject.Should().NotBe(IPAddress.None);
            }
        }

        /// <summary>
        /// gateway get physical address.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_GatewayPhysicalAddress()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.GatewayPhysicalAddress))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.GatewayPhysicalAddress} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                return;
            }

            PhysicalAddress? physicalAddress = this.NetworkGatewayInfo.GatewayPhysicalAddress;
            this.TestOutputHelper.WriteLine($"GatewayNetworkAddressInfo : {this.NetworkGatewayInfo.GatewayNetworkAddressInfo?.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine)}");

            physicalAddress.Should().NotBeNull().And.Subject.Should().NotBe(PhysicalAddress.None);
            this.TestOutputHelper.WriteLine($"Gateway Physical Address: {physicalAddress.ToDashString()}");
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_ToStringWithParents_Init.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_ToStringWithParents_Init()
        {
            this.TestOutputHelper.WriteLine(
                $"Network Gateway : {this.NetworkGatewayInfo.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropName)}");
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_ToStringWithParentsMultiLine_Init.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_ToStringWithParentsMultiLine_Init()
        {
            this.TestOutputHelper.WriteLine(
                $"Network Gateway : {this.NetworkGatewayInfo.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine)}");
        }

        /// <summary>
        /// Defines the test method NetworkGatewayInfo_ToStringWithParents_Critical.
        /// </summary>
        [Fact]
        public void NetworkGatewayInfo_ToStringWithParents_Critical()
        {
            this.NetworkGatewayInfo.ToString(CoreLoggableFormatFlags.ToStringWithParentsMultiLine, LogLevel.Critical).Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test method NetworkInterface_OutputAllSortedActiveNetworkInterfacesWithGateway.
        /// </summary>
        [Fact]
        public void NetworkInterface_OutputAllActiveNetworkInterfacesWithGateway()
        {
            ICoreNetworkAddressInfo? gatewayNetworkInfo = this.NetworkGatewayInfo.GatewayNetworkAddressInfo;

            gatewayNetworkInfo.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"GatewayNetworkInfo:\n{gatewayNetworkInfo.ToStringWithPropNameMultiLine()}");
            this.TestOutputHelper.WriteLine();

            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"Preferred Network Interface:\n{this.NetworkGatewayInfo.PreferredGatewayNetworkInterface.ToStringWithPropNameMultiLine()}");
            this.TestOutputHelper.WriteLine();

            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.PreferredIPAddressSubnet.Should().NotBeNull();
            gatewayNetworkInfo!.IPAddressSubnet.CanGatewayRouteIPAddress(this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.PreferredIPAddress!).Should().BeTrue();
            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.PreferredSubnetMask.Should().Be(gatewayNetworkInfo.SubnetMask);

            foreach (ICoreNetworkInterface? networkInterface in this.TestNetworkingSystem.GetAllActiveNetworkInterfacesWithGateway())
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method NetworkInterface_OutputAllNetworkInterfacesWithGateway.
        /// </summary>
        [Fact]
        public void NetworkInterface_OutputAllNetworkInterfacesWithGateway()
        {
            ICoreNetworkAddressInfo? gatewayNetworkInfo = this.NetworkGatewayInfo.GatewayNetworkAddressInfo;

            gatewayNetworkInfo.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"GatewayNetworkInfo:\n{gatewayNetworkInfo.ToStringWithPropNameMultiLine()}");
            this.TestOutputHelper.WriteLine();

            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"Preferred Network Interface:\n{this.NetworkGatewayInfo.PreferredGatewayNetworkInterface.ToStringWithPropNameMultiLine()}");
            this.TestOutputHelper.WriteLine();

            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.PreferredIPAddressSubnet.Should().NotBeNull();
            gatewayNetworkInfo!.IPAddressSubnet.CanGatewayRouteIPAddress(this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.PreferredIPAddress).Should().BeTrue();
            this.NetworkGatewayInfo.PreferredGatewayNetworkInterface!.PreferredSubnetMask.Should().Be(gatewayNetworkInfo.SubnetMask);

            foreach (ICoreNetworkInterface? networkInterface in this.TestNetworkingSystem.GetAllNetworkInterfacesWithGateway())
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method NetworkEnumeratorGateway_Ctor.
        /// </summary>
        [Fact]
        public void NetworkEnumeratorGateway_Ctor()
        {
            var networkEnumeratorGateway = new CoreGatewayNetworkCollection(this.NetworkGatewayInfo);
            networkEnumeratorGateway.Should().NotBeNull();
        }

        /// <summary>
        /// Defines the test method NetworkEnumeratorGateway_IPAddressRange.
        /// </summary>
        [Fact]
        public void NetworkEnumeratorGateway_IPAddressRange()
        {
            this.TestOutputHelper.WriteLine($"Gateway: {this.NetworkGatewayInfo.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine)}");

            var networkEnumeratorGateway = new CoreGatewayNetworkCollection(this.NetworkGatewayInfo);
            networkEnumeratorGateway.Should().NotBeNull();

            networkEnumeratorGateway.IPAddressRange.IpAddressLowerInclusive.Should()
                .Be(this.NetworkGatewayInfo.GatewayIPAddressSubnet?.NetworkAddress.Increment());
            networkEnumeratorGateway.IPAddressRange.IpAddressUpperInclusive.Should()
                .Be(this.NetworkGatewayInfo.GatewayIPAddressSubnet?.BroadcastAddress);
        }

        /// <summary>
        /// Defines the test method NetworkEnumeratorGateway_Enumerator.
        /// </summary>
        [Fact(Skip = "Expensive")]
        public void NetworkEnumeratorGateway_Enumerator()
        {
            var networkEnumeratorGateway = new CoreGatewayNetworkCollection(this.NetworkGatewayInfo);
            networkEnumeratorGateway.Should().NotBeNull();

            var count = 0;

            foreach (IPAddress ipAddress in networkEnumeratorGateway)
            {
                count++;
                this.TestOutputHelper.WriteLine($"IPAddress{count}:\t{ipAddress}");
            }
        }

        private ICoreNetworkGatewayInfo CreateNetworkGatewayInfo()
        {
            ICoreNetworkGatewayInfo? networkGatewayInfo = this.TestNetworkServices.PreferredNetworkGatewayInfo;
            networkGatewayInfo.Should().NotBeNull();
            networkGatewayInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkGatewayInfo>();

            return networkGatewayInfo!;
        }
    }
}
