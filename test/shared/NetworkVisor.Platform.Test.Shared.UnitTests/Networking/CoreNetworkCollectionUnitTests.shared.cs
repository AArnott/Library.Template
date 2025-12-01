// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreNetworkCollectionUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking
{
    /// <summary>
    /// Class CoreNetworkCollectionUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreNetworkCollectionUnitTests))]

    public class CoreNetworkCollectionUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkCollectionUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkCollectionUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method NetworkCollection_PrivateClassC.
        /// </summary>
        [Fact]
        public void NetworkCollection_PrivateClassC()
        {
            var networkCollection = new CoreNetworkCollection(IPAddress.Parse("192.168.168.10").ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC));
            networkCollection.Count().Should().Be(256);
        }

        /// <summary>
        /// Defines the test method NetworkCollection_PrivateClassB.
        /// </summary>
        [Fact]
        public void NetworkCollection_PrivateClassB()
        {
            var networkCollection = new CoreNetworkCollection(CoreIPAddressExtensions.Private192IPAddressSubNet);
            networkCollection.Count().Should().Be(65536);
            networkCollection.First().Should().Be(IPAddress.Parse("192.168.0.0"));
            networkCollection.IPAddressRange.IsInRange(IPAddress.Parse("10.1.10.5")).Should().BeFalse();
            networkCollection.IPAddressRange.IsInRange(IPAddress.Parse("192.168.168.10")).Should().BeTrue();
            networkCollection.Last().Should().Be(IPAddress.Parse("192.168.255.255"));

            foreach (IPAddress ipAddress in networkCollection)
            {
                networkCollection.IPAddressRange.IsInRange(ipAddress).Should().BeTrue();
            }
        }

        /// <summary>
        /// Defines the test method NetworkCollection_PrivateClassA.
        /// </summary>
        [Fact(Skip = "Expensive")]
        [ExcludeFromCodeCoverage]
        public void NetworkCollection_PrivateClassA()
        {
            var networkCollection = new CoreNetworkCollection(CoreIPAddressExtensions.Private10IPAddressSubNet);
            networkCollection.Count().Should().Be(16777216);
            networkCollection.First().Should().Be(IPAddress.Parse("10.0.0.0"));
            networkCollection.IPAddressRange.IsInRange(IPAddress.Parse("10.1.10.5")).Should().BeTrue();
            networkCollection.IPAddressRange.IsInRange(IPAddress.Parse("192.168.168.10")).Should().BeFalse();
            networkCollection.Last().Should().Be(IPAddress.Parse("10.255.255.255"));
        }

        /// <summary>
        /// Defines the test method NetworkCollection_Ctor_Null.
        /// </summary>
        [Fact]
        public void NetworkCollection_Ctor_Null()
        {
            Func<CoreNetworkCollection> fx = () => new CoreNetworkCollection(null!);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ipAddressSubnet");
        }

        /// <summary>
        /// Defines the test method NetworkCollection_BadAddressFamily.
        /// </summary>
        [Fact]
        public void NetworkCollection_BadAddressFamily()
        {
            var networkCollection = new CoreNetworkCollection(CoreIPAddressExtensions.Private10IPAddressSubNet);
            networkCollection.IPAddressRange.IsInRange(IPAddress.IPv6Loopback).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method NetworkCollection_IsInRange_Null.
        /// </summary>
        [Fact]
        public void NetworkCollection_IsInRange_Null()
        {
            var networkCollection = new CoreNetworkCollection(CoreIPAddressExtensions.Private10IPAddressSubNet);
            Func<bool> fx = () => networkCollection.IPAddressRange.IsInRange(null!);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("address");
        }

        /// <summary>
        /// Defines the test method NetworkCollection_IPAddressRangeCollection_IPAddress.
        /// </summary>
        [Fact]
        public void NetworkCollection_IPAddressRangeCollection_IPAddress()
        {
            var ipAddressRangeCollection = new CoreIPAddressRangeCollection(IPAddress.Any, IPAddress.Broadcast);

            ipAddressRangeCollection.IpAddressLowerInclusive.Should().Be(IPAddress.Any);
            ipAddressRangeCollection.IpAddressUpperInclusive.Should().Be(IPAddress.Broadcast);
            ((IEnumerable)ipAddressRangeCollection).GetEnumerator().Should().BeAssignableTo<IEnumerator<IPAddress>>();
        }
    }
}
