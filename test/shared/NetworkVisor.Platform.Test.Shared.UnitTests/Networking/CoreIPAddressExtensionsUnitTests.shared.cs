// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreIPAddressExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking
{
    /// <summary>
    /// Class CoreIPAddressExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreIPAddressExtensionsUnitTests))]

    public class CoreIPAddressExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreIPAddressExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreIPAddressExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_SubnetMaskFromHostsNumber.
        /// </summary>
        /// <param name="numberOfHosts">The number of hosts.</param>
        /// <param name="subnetMaskString">The subnet mask string.</param>
        [Theory]
        [InlineData(100, "255.255.255.128")]
        [InlineData(200, CoreIPAddressExtensions.StringSubnetClassC)]
        [InlineData(254, CoreIPAddressExtensions.StringSubnetClassC)]
        [InlineData(255, "255.255.254.0")]
        [InlineData(256, "255.255.254.0")]
        [InlineData(1000, "255.255.252.0")]
        [InlineData(8190, "255.255.224.0")]
        [InlineData(16382, "255.255.192.0")]
        [InlineData(32766, "255.255.128.0")]
        [InlineData(32767, "255.255.0.0")]
        [InlineData(66767, "255.254.0.0")]
        public void IPAddressExtensions_SubnetMaskFromHostsNumber(int numberOfHosts, string subnetMaskString)
        {
            IPAddress subnetMask = CoreIPAddressExtensions.SubnetMaskFromHostsNumber(numberOfHosts);
            subnetMask.Should().Be(IPAddress.Parse(subnetMaskString));
            this.TestOutputHelper.WriteLine($"SubnetMask: {subnetMask}");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_SubnetMaskFromNetBitLength.
        /// </summary>
        /// <param name="netPartLength">Length of the net part.</param>
        /// <param name="subnetMaskString">The subnet mask string.</param>
        [Theory]
        [InlineData(2, "192.0.0.0")]
        [InlineData(3, "224.0.0.0")]
        [InlineData(4, "240.0.0.0")]
        [InlineData(5, "248.0.0.0")]
        [InlineData(6, "252.0.0.0")]
        [InlineData(7, "254.0.0.0")]
        [InlineData(8, "255.0.0.0")]
        [InlineData(9, "255.128.0.0")]
        [InlineData(10, "255.192.0.0")]
        [InlineData(11, "255.224.0.0")]
        [InlineData(12, "255.240.0.0")]
        [InlineData(13, "255.248.0.0")]
        [InlineData(14, "255.252.0.0")]
        [InlineData(15, "255.254.0.0")]
        [InlineData(16, "255.255.0.0")]
        [InlineData(17, "255.255.128.0")]
        [InlineData(18, "255.255.192.0")]
        [InlineData(19, "255.255.224.0")]
        [InlineData(20, "255.255.240.0")]
        [InlineData(21, "255.255.248.0")]
        [InlineData(22, "255.255.252.0")]
        [InlineData(23, "255.255.254.0")]
        [InlineData(24, "255.255.255.0")]
        [InlineData(25, "255.255.255.128")]
        [InlineData(26, "255.255.255.192")]
        [InlineData(27, "255.255.255.224")]
        [InlineData(28, "255.255.255.240")]
        [InlineData(29, "255.255.255.248")]
        [InlineData(30, "255.255.255.252")]
        [InlineData(31, "255.255.255.254")]
        [InlineData(32, CoreIPAddressExtensions.StringBroadcast)]
        public void IPAddressExtensions_SubnetMaskFromNetBitLength(int netPartLength, string subnetMaskString)
        {
            IPAddress subnetMask = CoreIPAddressExtensions.SubnetMaskFromNetBitLength(netPartLength);
            this.TestOutputHelper.WriteLine($"SubnetMask: {subnetMask}");
            subnetMask.Should().Be(IPAddress.Parse(subnetMaskString));
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_SubnetMaskFromHostBitLength_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_SubnetMaskFromHostBitLength_Null()
        {
            Func<IPAddress> fx = () => CoreIPAddressExtensions.SubnetMaskFromHostBitLength(31);
            fx.Should().Throw<ArgumentException>().And.Message.Should().Be("Too many network hosts for IPV4");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_HostsFromSubnetMask_ClassC.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_HostsFromSubnetMask_ClassC()
        {
            uint hostsCount = CoreIPAddressExtensions.SubnetClassC.CountHostsFromSubnetMask();
            this.TestOutputHelper.WriteLine($"SubnetMask: {CoreIPAddressExtensions.SubnetClassC}, Hosts: {hostsCount}");
            hostsCount.Should().Be(254);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_HostsFromSubnetMask_ClassC.
        /// </summary>
        /// <param name="ipAddressStringToTest">IPAddress to test as a string.</param>
        /// <param name="ipAddressStringExpected">Expected IPAddress result as a string.</param>
        [Theory]
        [InlineData(null, null)]
        [InlineData("fd52:1ece:1f8:e4e:8c:ffc9:b65f:1978", "fd52:1ece:1f8:e4e:8c:ffc9:b65f:1978")]
        [InlineData("fe80::286c:703a:9fb:325f%2", "fe80::286c:703a:9fb:325f")]
        [InlineData("fe80::95d8:2a40:6a35:42fa%33", "fe80::95d8:2a40:6a35:42fa")]
        [InlineData("fe80::ddb1:af2:511a:36dd%29", "fe80::ddb1:af2:511a:36dd")]
        [InlineData("fe80::%28", "fe80::")]
        public void IPAddressExtensions_WithoutScopeId(string? ipAddressStringToTest, string? ipAddressStringExpected)
        {
            IPAddress? ipAddressToTest = string.IsNullOrEmpty(ipAddressStringToTest) ? null : IPAddress.Parse(ipAddressStringToTest);
            IPAddress? ipAddressWithOutScopeId = ipAddressToTest?.ToWithoutScopeId();
            IPAddress? ipAddressExpected = string.IsNullOrEmpty(ipAddressStringExpected) ? null : IPAddress.Parse(ipAddressStringExpected);

            this.TestOutputHelper.WriteLine($"{ipAddressStringToTest} => {ipAddressExpected}");
            ipAddressWithOutScopeId.Should().Be(ipAddressExpected);
            ipAddressToTest.CompareTo(ipAddressExpected).Should().Be(0);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_HostsFromSubnetMask.
        /// </summary>
        /// <param name="subnetMaskString">The subnet mask string.</param>
        /// <param name="hostsCountExpected">The hosts count expected.</param>
        [Theory]
        [InlineData("255.255.255.0", 254)]
        [InlineData("255.255.0.0", 65534)]
        [InlineData("255.0.0.0", 16777214)]
        public void IPAddressExtensions_HostsFromSubnetMask(string subnetMaskString, uint hostsCountExpected)
        {
            var subnetMask = IPAddress.Parse(subnetMaskString);
            uint hostsCount = subnetMask.CountHostsFromSubnetMask();
            this.TestOutputHelper.WriteLine($"SubnetMask: {subnetMask}, Hosts: {hostsCount}");
            hostsCount.Should().Be(hostsCountExpected);
            subnetMask.CountAddressesFromSubnetMask().Should().Be(hostsCount + 2);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_AddressesFromSubnetMaskHostsFromSubnetMask.
        /// </summary>
        /// <param name="subnetMaskString">The subnet mask string.</param>
        /// <param name="addressesExpected">The addresses expected.</param>
        [Theory]
        [InlineData("255.255.255.0", 256)]
        [InlineData("255.255.0.0", 65536)]
        [InlineData("255.0.0.0", 16777216)]
        public void IPAddressExtensions_AddressesFromSubnetMaskHostsFromSubnetMask(string subnetMaskString, uint addressesExpected)
        {
            var subnetMask = IPAddress.Parse(subnetMaskString);
            uint addresses = CoreIPAddressExtensions.CountAddressesFromSubnetMask(subnetMask);
            this.TestOutputHelper.WriteLine($"SubnetMask: {subnetMask}, Addresses: {addresses}");
            addresses.Should().Be(addressesExpected);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_IsPrivateNetworkAddress.
        /// </summary>
        /// <param name="ipAddressString">IPAddress to test.</param>
        /// <param name="expected">Whether or not IPAddress is a private network address.</param>
        [Theory]
        [InlineData("192.168.168.10", true)]
        [InlineData("10.10.10.10", true)]
        [InlineData("172.16.10.0", true)]
        [InlineData("169.254.10.0", true)]
        [InlineData("8.8.8.8", false)]
        [InlineData(CoreIPAddressExtensions.StringLoopback, false)]
        [InlineData(CoreIPAddressExtensions.StringNonRoutable, false)]
        public void IPAddressExtensions_IsPrivateNetworkAddress(string ipAddressString, bool expected)
        {
            IPAddress.Parse(ipAddressString).IsPrivateNetworkAddress().Should().Be(expected);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_CanGatewayRouteIPAddressSubnet.
        /// </summary>
        /// <param name="ipAddressStringGateway">Gateway IPAddress to test.</param>
        /// <param name="subnetMaskStringGateway">Gateway Subnet mask to test.</param>
        /// <param name="ipAddressStringRoutable">Routable IPAddress.</param>
        /// <param name="expected">Whether gateway can route ipaddress.</param>
        [Theory]
        [InlineData("192.168.168.0", CoreIPAddressExtensions.StringSubnetClassC, "192.168.168.10", true)]
        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassA, "10.10.10.10", true)]
        [InlineData("172.16.0.0", CoreIPAddressExtensions.StringSubnetClassB, "172.16.10.0", true)]
        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassC, "169.254.10.0", false)]
        [InlineData("10.0.0.0", "240.0.0.0", "192.168.168.10", false)]
        [InlineData(CoreIPAddressExtensions.StringMerakiDHCP, CoreIPAddressExtensions.StringSubnetClassA, "10.0.0.0", true)]
        [InlineData(CoreIPAddressExtensions.StringSubnetClassA, "8.8.8.8", "192.168.168.0", false)]
        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassA, "::ffff:a0a:a0a", false)]

        [InlineData(CoreIPAddressExtensions.StringAny, CoreIPAddressExtensions.StringBroadcast, "10.10.10.10", false)]
        [InlineData("10.10.0.0", CoreIPAddressExtensions.StringSubnetClassA, "10.10.10.10", true)]
        [InlineData("10.10.0.0", CoreIPAddressExtensions.StringSubnetClassB, "10.10.10.10", true)]
        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassC, "10.10.10.10", false)]

        [InlineData("10.1.0.0", CoreIPAddressExtensions.StringBroadcast, "10.10.10.10", false)]
        [InlineData("10.1.0.0", CoreIPAddressExtensions.StringSubnetClassA, "10.10.10.10", true)]
        [InlineData("10.1.0.0", CoreIPAddressExtensions.StringSubnetClassB, "10.10.10.10", false)]
        [InlineData("10.1.0.0", CoreIPAddressExtensions.StringSubnetClassC, "10.10.10.10", false)]

        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassA, "10.1.10.32", true)]
        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassB, "10.1.10.32", false)]
        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassC, "10.1.10.32", false)]

        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassA, "10.0.10.32", true)]
        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassB, "10.0.10.32", true)]
        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassC, "10.0.10.32", false)]

        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassA, "10.0.0.32", true)]
        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassB, "10.0.0.32", true)]
        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringSubnetClassC, "10.0.0.32", true)]

        [InlineData("10.0.0.0", CoreIPAddressExtensions.StringAny, "10.10.10.10", false)]
        [InlineData("10.1.0.0", "255.0.0.0", "192.168.168.10", false)]
        [InlineData("10.1.0.0", CoreIPAddressExtensions.StringBroadcast, "192.168.168.10", false)]
        public void IPAddressExtensions_CanGatewayRouteIPAddress(string ipAddressStringGateway, string subnetMaskStringGateway, string ipAddressStringRoutable, bool expected)
        {
            IPAddress.Parse(ipAddressStringGateway).ToIPAddressSubnet(IPAddress.Parse(subnetMaskStringGateway)).CanGatewayRouteIPAddress(IPAddress.Parse(ipAddressStringRoutable)).Should().Be(expected);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_CanGatewayRouteIPAddress_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_CanGatewayRouteIPAddress_Null()
        {
            Func<bool> fx = () => ((CoreIPAddressSubnet?)null).CanGatewayRouteIPAddress(IPAddress.Any);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("gatewayIPAddressSubnet");

            fx = () => CoreIPAddressSubnet.None.CanGatewayRouteIPAddress(null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ipAddress");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_Increment_Overflow.
        /// </summary>
        /// <param name="ipAddressString">IPAddress to test.</param>
        /// <param name="ipAddressStringExpected">IPAddress expected.</param>
        [Theory]
        [InlineData("10.1.10.1", "10.1.10.2")]
        [InlineData("10.1.10.255", "10.1.11.0")]
        [InlineData("10.1.255.255", "10.2.0.0")]
        [InlineData("10.255.255.255", "11.0.0.0")]
        [InlineData(CoreIPAddressExtensions.StringBroadcast, CoreIPAddressExtensions.StringAny)]
        public void IPAddressExtensions_Increment_Overflow(string ipAddressString, string ipAddressStringExpected)
        {
            IPAddress.Parse(ipAddressString).Increment().Should().Be(IPAddress.Parse(ipAddressStringExpected));
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_Increment_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_Increment_Null()
        {
            Func<IPAddress?> fx = () => ((IPAddress?)null).Increment();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("address");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_Decrement_Overflow.
        /// </summary>
        /// <param name="ipAddressString">IPAddress to test.</param>
        /// <param name="ipAddressStringExpected">IPAddress expected.</param>
        [Theory]
        [InlineData("10.1.10.2", "10.1.10.1")]
        [InlineData("10.1.11.0", "10.1.10.255")]
        [InlineData("10.2.0.0", "10.1.255.255")]
        [InlineData("11.0.0.0", "10.255.255.255")]
        [InlineData(CoreIPAddressExtensions.StringAny, CoreIPAddressExtensions.StringBroadcast)]
        public void IPAddressExtensions_Decrement_Overflow(string ipAddressString, string ipAddressStringExpected)
        {
            IPAddress.Parse(ipAddressString).Decrement().Should().Be(IPAddress.Parse(ipAddressStringExpected));
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_Decrement_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_Decrement_Null()
        {
            Func<IPAddress?> fx = () => ((IPAddress?)null).Decrement();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("address");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_ToLong.
        /// </summary>
        /// <param name="ipAddressString">IPAddress to test.</param>
        /// <param name="longExpected">long result expected.</param>
        [Theory]
        [InlineData("10.1.10.1", 17432842L)]
        [InlineData("172.16.10.0", 659628L)]
        [InlineData("169.254.10.0", 720553L)]
        [InlineData("192.168.168.10", 178825408L)]
        [InlineData(CoreIPAddressExtensions.StringAny, 0)]
        [InlineData(CoreIPAddressExtensions.StringBroadcast, 4294967295L)]
        public void IPAddressExtensions_ToLong(string ipAddressString, long longExpected)
        {
            IPAddress.Parse(ipAddressString).ToLong().Should().Be(longExpected);
            new IPAddress(longExpected).ToLong().Should().Be(longExpected);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_ToLong_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_ToLong_Null()
        {
            Func<long?> fx = () => ((IPAddress?)null).ToLong();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("address");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_ToLongBackwards.
        /// </summary>
        /// <param name="ipAddressString">IPAddress to test.</param>
        /// <param name="longExpected">long result expected.</param>
        [Theory]
        [InlineData("10.1.10.1", 167840257L)]
        [InlineData("172.16.10.0", 2886732288L)]
        [InlineData("169.254.10.0", 2851998208L)]
        [InlineData("192.168.168.10", 3232278538L)]
        [InlineData(CoreIPAddressExtensions.StringAny, 0)]
        [InlineData(CoreIPAddressExtensions.StringBroadcast, 4294967295L)]
        public void IPAddressExtensions_ToLongBackwards(string ipAddressString, long longExpected)
        {
            IPAddress.Parse(ipAddressString).ToLongBackwards().Should().Be(longExpected);
            new IPAddress(longExpected).ToLongBackwards().Should().Be(IPAddress.Parse(ipAddressString).ToLong());
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_ToLongBackwards_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_ToLongBackwards_Null()
        {
            Func<long?> fx = () => ((IPAddress?)null).ToLongBackwards();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("address");
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_Comparisons.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_Comparisons()
        {
            IPAddress ipAddressSmall = CoreIPAddressExtensions.NonRoutableIPAddress;
            IPAddress ipAddressSmall2 = CoreIPAddressExtensions.NonRoutableIPAddress;
            IPAddress ipAddressLarge = ipAddressSmall.Increment();

            ipAddressSmall.CompareTo(ipAddressSmall2).Should().Be(0);
            ipAddressSmall.CompareTo(ipAddressLarge).Should().BeLessThan(0);

            ipAddressLarge.CompareTo(ipAddressSmall).Should().BeGreaterThan(0);

            ipAddressSmall.Equals(ipAddressSmall2).Should().BeTrue();
            ipAddressSmall.Equals(ipAddressLarge).Should().BeFalse();

            ipAddressSmall.Equals((object?)ipAddressSmall2).Should().BeTrue();
            ipAddressSmall.Equals((object?)ipAddressLarge).Should().BeFalse();

            ((IPAddress?)null).CompareTo(null).Should().Be(0);
            IPAddress.Any.CompareTo(IPAddress.IPv6Any).Should().BeLessThan(0);

            Func<int> fx = () => ((IPAddress?)null).CompareTo(ipAddressSmall);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("x");

            ipAddressSmall.CompareTo(null).Should().Be(1);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_GetNetworkAddress_IPv6.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_GetNetworkAddress_IPv6()
        {
            IPAddress.IPv6Any.GetNetworkAddress(CoreIPAddressExtensions.SubnetClassC).Should().Be(IPAddress.Any);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_GetNetworkAddress_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_GetNetworkAddress_Null()
        {
            Func<IPAddress> fx = () => ((IPAddress?)null).GetNetworkAddress(CoreIPAddressExtensions.SubnetClassC);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("address");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_GetNetworkAddress_Null_Subnet.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_GetNetworkAddress_Null_Subnet()
        {
            Func<IPAddress> fx = () => IPAddress.Any.GetNetworkAddress(null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("subnetMask");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_BroadcastAddress_IPv6.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_BroadcastAddress_IPv6()
        {
            IPAddress.IPv6Any.GetBroadcastAddress(CoreIPAddressExtensions.SubnetClassC).Should().Be(IPAddress.Any.GetBroadcastAddress(CoreIPAddressExtensions.SubnetClassC));
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_BroadcastAddress_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_BroadcastAddress_Null()
        {
            Func<IPAddress?> fx = () => ((IPAddress?)null).GetBroadcastAddress(CoreIPAddressExtensions.SubnetClassC);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("address");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_BroadcastAddress_Null_Subnet.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_BroadcastAddress_Null_Subnet()
        {
            Func<IPAddress?> fx = () => IPAddress.Any.GetBroadcastAddress(null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("subnetMask");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_CountAddressesFromSubnetMask_Null_Subnet.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_CountAddressesFromSubnetMask_Null_Subnet()
        {
            Func<uint> fx = () => ((IPAddress?)null).CountAddressesFromSubnetMask();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("subnetMask");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_CountHostsFromSubnetMask_Null_Subnet.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_CountHostsFromSubnetMask_Null_Subnet()
        {
            Func<uint> fx = () => ((IPAddress?)null).CountHostsFromSubnetMask();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("subnetMask");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_IsMerakiDHCPAddress_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_IsMerakiDHCPAddress_Null()
        {
            Func<bool> fx = () => ((IPAddress?)null).IsMerakiDHCPAddress();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ipAddress");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_IsAddressOnSameSubnet_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_IsAddressOnSameSubnet_Null()
        {
            Func<bool> fx = () => ((IPAddress?)null).IsAddressOnSameSubnet(IPAddress.Any, CoreIPAddressExtensions.SubnetClassC);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("address1");

            fx = () => IPAddress.Any.IsAddressOnSameSubnet(null, CoreIPAddressExtensions.SubnetClassC);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("address2");

            fx = () => IPAddress.Any.IsAddressOnSameSubnet(IPAddress.Any, null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("subnetMask");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_ToLongBackwards.
        /// </summary>
        /// <param name="networkAddressString">IPAddress to test.</param>
        /// <param name="ipAddressStringExpected">ipAddress expected.</param>
        /// <param name="subnetMaskStringExpected">subnet expected.</param>
        [Theory]
        [InlineData("192.168.200.0/24", "192.168.200.0", "255.255.255.0")]
        [InlineData("192.168.232.0/21", "192.168.232.0", "255.255.248.0")]
        [InlineData("10.1.10.0/16", "10.1.10.0", "255.255.0.0")]
        [InlineData("10.1.10.0/32", "10.1.10.0", CoreIPAddressExtensions.StringBroadcast)]
        [InlineData("172.16.17.30/20", "172.16.17.30", "255.255.240.0")]
        [InlineData("", CoreIPAddressExtensions.StringBroadcast, CoreIPAddressExtensions.StringBroadcast)]
        [InlineData(null, CoreIPAddressExtensions.StringBroadcast, CoreIPAddressExtensions.StringBroadcast)]
        public void IPAddressExtensions_ParseNetworkAddress(string? networkAddressString, string ipAddressStringExpected, string subnetMaskStringExpected)
        {
            CoreIPAddressExtensions.ParseNetworkAddress(networkAddressString).Should().Be(IPAddress.Parse(ipAddressStringExpected).ToIPAddressSubnet(IPAddress.Parse(subnetMaskStringExpected)));
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_IsBroadcastAddress.
        /// </summary>
        /// <param name="ipAddressString">IPAddress to test.</param>
        /// <param name="expected">Whether or not IPAddress is a broadcast network address.</param>
        [Theory]
        [InlineData(CoreIPAddressExtensions.StringBroadcast, true)]
        [InlineData("224.0.0.0", false)]
        [InlineData("192.168.168.10", false)]
        [InlineData("10.10.10.10", false)]
        [InlineData("172.16.10.0", false)]
        [InlineData("169.254.10.0", false)]
        [InlineData("8.8.8.8", false)]
        [InlineData(CoreIPAddressExtensions.StringLoopback, false)]
        [InlineData(CoreIPAddressExtensions.StringNonRoutable, false)]
        public void IPAddressExtensions_IsBroadcastAddress(string ipAddressString, bool expected)
        {
            IPAddress.Parse(ipAddressString).IsBroadcastAddress().Should().Be(expected);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_IsBroadcastAddress_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_IsBroadcastAddress_Null()
        {
            Func<bool> fx = () => ((IPAddress?)null).IsBroadcastAddress();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ipAddress");
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_IsReservedAddress.
        /// </summary>
        /// <param name="ipAddressString">IPAddress to test.</param>
        /// <param name="expected">Whether or not IPAddress is a reserved network address.</param>
        [Theory]
        [InlineData(CoreIPAddressExtensions.StringBroadcast, true)]
        [InlineData("224.0.0.0", true)]
        [InlineData("192.168.168.10", false)]
        [InlineData("10.10.10.10", false)]
        [InlineData("172.16.10.0", false)]
        [InlineData("169.254.10.0", false)]
        [InlineData("8.8.8.8", false)]
        [InlineData(CoreIPAddressExtensions.StringLoopback, false)]
        [InlineData(CoreIPAddressExtensions.StringNonRoutable, false)]
        public void IPAddressExtensions_IsReservedAddress(string ipAddressString, bool expected)
        {
            IPAddress.Parse(ipAddressString).IsReservedAddress().Should().Be(expected);
        }

        /// <summary>
        /// Defines the test method IPAddressExtensions_IsReservedAddresss_Null.
        /// </summary>
        [Fact]
        public void IPAddressExtensions_IsReservedAddresss_Null()
        {
            Func<bool> fx = () => ((IPAddress?)null).IsReservedAddress();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ipAddress");
        }
    }
}
