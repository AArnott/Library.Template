// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests.Networking
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreIPAddressSubnetUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Logging.Types;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking
{
    /// <summary>
    /// Class CoreIPAddressSubnetUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreIPAddressSubnetUnitTests))]

    public class CoreIPAddressSubnetUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreIPAddressSubnetUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreIPAddressSubnetUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_Ctor.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_Ctor()
        {
            CoreIPAddressSubnet ipAddressSubnet = CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassA);
            ipAddressSubnet.IPAddress.Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);
            ipAddressSubnet.SubnetMask.Should().Be(CoreIPAddressExtensions.SubnetClassA);
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_Ctor_Null.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_Ctor_Null()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            ((CoreIPAddressSubnet)null).IsNullOrNone().Should().BeTrue();
            ((CoreIPAddressSubnet)null).IsNone().Should().BeFalse();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_GetHashCode.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_GetHashCode()
        {
            CoreIPAddressSubnet ipAddressSubnetSmall = CoreIPAddressExtensions.NonRoutableIPAddress.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC);
            CoreIPAddressSubnet ipAddressSubnetLarge = ipAddressSubnetSmall.IPAddress.Increment().ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC);

            ipAddressSubnetSmall.GetHashCode().Should().NotBe(ipAddressSubnetLarge.GetHashCode());
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_GetLogPropertyListLevel_Null.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_GetLogPropertyListLevel_Null()
        {
            Func<IEnumerable<ICoreLogPropertyLevel>> fx = () => CoreIPAddressExtensions.NonRoutableIPAddress.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC).GetLogPropertyListLevel(null!);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_FormatLogString_Null.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_FormatLogString_Null()
        {
            Func<StringBuilder> fx = () => CoreIPAddressExtensions.NonRoutableIPAddress.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC).FormatLogString(null, null!);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_ToString.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_ToString()
        {
            CoreIPAddressExtensions.NonRoutableIPAddress.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC).ToString().Should().Be("203.0.113.1,255.255.255.0");
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_ToString_Formatter.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_ToString_Formatter_LogLevel()
        {
            CoreIPAddressExtensions.NonRoutableIPAddress.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC).ToString(new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToString, LogLevel.Trace)).Should().Be("203.0.113.1,255.255.255.0");
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_ToStringWithPropName.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_ToStringWithPropName()
        {
            CoreIPAddressExtensions.NonRoutableIPAddress.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC).ToString(new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToStringWithPropName)).Should().Be("IPAddress=203.0.113.1,SubnetMask=255.255.255.0");
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_ToString_Null.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_ToString_Null()
        {
            Func<string> fx = () => new IPHostEntry().ToString(null!);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_GetHashCode.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_NetworkAddressSubnet()
        {
            CoreIPAddressSubnet networkAddressSubnet = CoreIPAddressExtensions.NonRoutableIPAddress.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC).NetworkAddressSubnet;
            networkAddressSubnet.IsNullOrNone().Should().BeFalse();
            networkAddressSubnet.IsNone().Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_IsAddressOnSameSubnet.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_IsAddressOnSameSubnet()
        {
            CoreIPAddressExtensions.Private10IPAddressSubNet.IsAddressOnSameSubnet(IPAddress.Parse("10.10.10.10")).Should().BeTrue();
            CoreIPAddressExtensions.Private10IPAddressSubNet.IsAddressOnSameSubnet(IPAddress.Parse("192.168.168.10")).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method IPAddressSubnet_Comparisons.
        /// </summary>
        [Fact]
        public void IPAddressSubnet_Comparisons()
        {
            CoreIPAddressSubnet? ipAddressSubnetSmall = CoreIPAddressExtensions.NonRoutableIPAddress.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC);
            CoreIPAddressSubnet? ipAddressSubnetSmall2 = CoreIPAddressExtensions.NonRoutableIPAddress.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC);
            CoreIPAddressSubnet ipAddressSubnetLarge = ipAddressSubnetSmall.IPAddress.Increment().ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC);

            (ipAddressSubnetSmall == ipAddressSubnetSmall2).Should().BeTrue();
            (ipAddressSubnetSmall == ipAddressSubnetLarge).Should().BeFalse();

#pragma warning disable CA1508 // Avoid dead conditional code
            (ipAddressSubnetSmall is null).Should().BeFalse();
#pragma warning disable SA1131 // Use readable conditions
            (ipAddressSubnetSmall2 is null).Should().BeFalse();
#pragma warning restore SA1131 // Use readable conditions
#pragma warning restore CA1508 // Avoid dead conditional code

            (ipAddressSubnetSmall != ipAddressSubnetSmall2).Should().BeFalse();
            (ipAddressSubnetSmall != ipAddressSubnetLarge).Should().BeTrue();

            (ipAddressSubnetLarge == ipAddressSubnetSmall).Should().BeFalse();
            (ipAddressSubnetLarge >= ipAddressSubnetSmall).Should().BeTrue();
            (ipAddressSubnetLarge > ipAddressSubnetSmall).Should().BeTrue();
            (ipAddressSubnetLarge < ipAddressSubnetSmall).Should().BeFalse();
            (ipAddressSubnetLarge <= ipAddressSubnetSmall).Should().BeFalse();

            (ipAddressSubnetSmall == ipAddressSubnetLarge).Should().BeFalse();
            (ipAddressSubnetSmall >= ipAddressSubnetLarge).Should().BeFalse();
            (ipAddressSubnetSmall > ipAddressSubnetLarge).Should().BeFalse();
            (ipAddressSubnetSmall < ipAddressSubnetLarge).Should().BeTrue();
            (ipAddressSubnetSmall <= ipAddressSubnetLarge).Should().BeTrue();

            ipAddressSubnetSmall!.CompareTo(ipAddressSubnetSmall2).Should().Be(0);
            ipAddressSubnetSmall.CompareTo(ipAddressSubnetLarge).Should().BeLessThan(0);

            ipAddressSubnetLarge.CompareTo(ipAddressSubnetSmall).Should().BeGreaterThan(0);

            ipAddressSubnetSmall.Equals(ipAddressSubnetSmall2).Should().BeTrue();
            ipAddressSubnetSmall.Equals(ipAddressSubnetLarge).Should().BeFalse();

            ipAddressSubnetSmall.Equals((object?)ipAddressSubnetSmall2).Should().BeTrue();
            ipAddressSubnetSmall.Equals((object?)ipAddressSubnetLarge).Should().BeFalse();

            ipAddressSubnetSmall.CompareTo((object?)ipAddressSubnetSmall2).Should().Be(0);
            ipAddressSubnetSmall.CompareTo((object?)ipAddressSubnetLarge).Should().BeLessThan(0);

            ipAddressSubnetLarge.CompareTo((object?)ipAddressSubnetSmall).Should().BeGreaterThan(0);

            Func<int> fx = () => ipAddressSubnetLarge.CompareTo((object?)new object());
            fx.Should().Throw<ArgumentException>().And.ParamName.Should().Be("obj");
        }
    }
}
