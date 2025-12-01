// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreNetworkEntityUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Networks;
using NetworkVisor.Core.Entities.Networks.Addresses;
using NetworkVisor.Core.Entities.Networks.Base;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Serialization;
using NetworkVisor.Platform.Test.TestCase;
using NetworkVisor.Platform.Test.TestEntities;
using Xunit;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(typeof(CoreXunitSerializer), typeof(CoreIPEndPoint), typeof(PhysicalAddress), typeof(CoreIPAddressSubnet), typeof(CoreObjectItem))]

namespace NetworkVisor.Platform.Test.Messaging.Shared.UnitTests.Entities
{
    /// <summary>
    /// Class CoreNetworkEntityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkEntityUnitTests))]
    public class CoreNetworkEntityUnitTests : CoreEntityTestCaseBase
    {
        public static (IPAddress, int, CoreNetworkAddressEntityType, CoreIPAddressScore)[] TestNetworkAddresses = CoreTestEntityConstants.TestNetworkAddresses;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreNetworkEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void NetworkEntityUnit_Default()
        {
            var networkEntity = new CoreNetworkEntity();
            this.ValidateAndOutputEntity<CoreNetworkEntity>(networkEntity, CoreEntityType.NetworkV1, (ulong)CoreIPAddressScore.None, CoreNetworkEntity.CreateLookupKey(networkEntity.NetworkAddress));

            // NetworkEntity specific validations
            _ = networkEntity.NetworkAddress.Should().Be(IPAddress.None);
            _ = networkEntity.NetworkAddressPrefixLength.Should().Be(32);
            _ = networkEntity.IPAddressScore.Should().Be(CoreIPAddressScore.None);
            _ = networkEntity.NetworkAddressEntityType.Should().Be(CoreNetworkAddressEntityType.NoneNetworkAddress);
        }

        [Fact]
        public void NetworkEntityUnit_Default_Same()
        {
            var networkEntity = new CoreNetworkEntity();
            this.ValidateAndOutputEntity<CoreNetworkEntity>(networkEntity, CoreEntityType.NetworkV1, (ulong)CoreIPAddressScore.None, CoreNetworkEntity.CreateLookupKey(networkEntity.NetworkAddress));
            var networkEntity2 = new CoreNetworkEntity();
            networkEntity.EntityID.Should().Be(networkEntity2.EntityID);
            networkEntity.DisplayName.Should().Be(networkEntity2.DisplayName);
            networkEntity.EntityType.Should().Be(networkEntity2.EntityType);
            networkEntity.EntityOwnerID.Should().Be(networkEntity2.EntityOwnerID);

            networkEntity.TimeToLive.Should().Be(networkEntity2.TimeToLive);
            networkEntity.Score.Should().Be(networkEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            networkEntity2.CreatedUtc = networkEntity.CreatedUtc;
            networkEntity2.ModifiedUtc = networkEntity.ModifiedUtc;
            networkEntity.Should().BeEquivalentTo(networkEntity2);

            networkEntity.Entity.Should().Be(networkEntity2.Entity);
        }

        [Theory]
        [MemberData(nameof(TestNetworkAddresses))]
        public void NetworkEntityUnit_Same(IPAddress networkAddress, int networkAddressPrefixLength, CoreNetworkAddressEntityType networkAddressEntityType, CoreIPAddressScore expectedScore)
        {
            var networkAddressEntity = new CoreNetworkAddressEntity(networkAddress, networkAddressPrefixLength);
            var networkEntity = new CoreNetworkEntity(networkAddressEntity, CoreNetworkEntityType.GenericNetwork);

            this.ValidateAndOutputEntity<CoreNetworkEntity>(networkEntity, CoreEntityType.NetworkV1, (ulong)expectedScore, CoreNetworkEntity.CreateLookupKey(networkEntity.NetworkAddress));
            networkEntity.NetworkAddressEntityType.Should().Be(networkAddressEntityType);
            networkEntity.NetworkAddress.Should().Be(networkAddress);
            networkEntity.NetworkAddressPrefixLength.Should().Be(CoreNetworkEntity.CalculateNetworkAddressPrefixLength(networkAddress, networkAddressPrefixLength));
            networkEntity.IPAddressScore.Should().Be(expectedScore);

            var networkEntity2 = new CoreNetworkEntity(networkAddressEntity, CoreNetworkEntityType.GenericNetwork);
            networkEntity.EntityID.Should().Be(networkEntity2.EntityID);
            networkEntity.DisplayName.Should().Be(networkEntity2.DisplayName);
            networkEntity.EntityType.Should().Be(networkEntity2.EntityType);
            networkEntity.EntityOwnerID.Should().Be(networkEntity2.EntityOwnerID);

            networkEntity.TimeToLive.Should().Be(networkEntity2.TimeToLive);
            networkEntity.Score.Should().Be(networkEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            networkEntity2.CreatedUtc = networkEntity.CreatedUtc;
            networkEntity2.ModifiedUtc = networkEntity.ModifiedUtc;
            networkEntity.Should().BeEquivalentTo(networkEntity2);

            networkEntity.Entity.Should().Be(networkEntity2.Entity);
        }
    }
}
