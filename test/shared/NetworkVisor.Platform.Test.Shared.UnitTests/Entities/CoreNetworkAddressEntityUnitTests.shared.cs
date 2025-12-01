// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreNetworkAddressEntityUnitTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Database.Providers.SQLite.Exceptions;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Networks.Addresses;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
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
    /// Class CoreNetworkAddressEntityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkAddressEntityUnitTests))]
    public class CoreNetworkAddressEntityUnitTests : CoreEntityTestCaseBase
    {
        public static (IPAddress, int, CoreNetworkAddressEntityType, CoreIPAddressScore)[] TestNetworkAddresses = CoreTestEntityConstants.TestNetworkAddresses;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkAddressEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreNetworkAddressEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void NetworkAddressEntityUnit_Default()
        {
            var networkAddressEntity = new CoreNetworkAddressEntity();
            this.ValidateAndOutputEntity<CoreNetworkAddressEntity>(networkAddressEntity, CoreEntityType.NetworkAddressV1, (ulong)CoreIPAddressScore.None, CoreNetworkAddressEntity.CreateLookupKey(networkAddressEntity.NetworkAddress));

            // NetworkAddress specific validations
            _ = networkAddressEntity.NetworkAddress.Should().Be(IPAddress.None);
            _ = networkAddressEntity.NetworkAddressPrefixLength.Should().Be(32);
            _ = networkAddressEntity.IPAddressScore.Should().Be(CoreIPAddressScore.None);
            _ = networkAddressEntity.NetworkAddressEntityType.Should().Be(CoreNetworkAddressEntityType.NoneNetworkAddress);
        }

        [Fact]
        public void NetworkAddressEntityUnit_Default_Same()
        {
            var networkAddressEntity = new CoreNetworkAddressEntity();
            this.ValidateAndOutputEntity<CoreNetworkAddressEntity>(networkAddressEntity, CoreEntityType.NetworkAddressV1, (ulong)CoreIPAddressScore.None, CoreNetworkAddressEntity.CreateLookupKey(networkAddressEntity.NetworkAddress));
            var networkAddressEntity2 = new CoreNetworkAddressEntity();
            networkAddressEntity.EntityID.Should().Be(networkAddressEntity2.EntityID);
            networkAddressEntity.DisplayName.Should().Be(networkAddressEntity2.DisplayName);
            networkAddressEntity.EntityType.Should().Be(networkAddressEntity2.EntityType);
            networkAddressEntity.EntityOwnerID.Should().Be(networkAddressEntity2.EntityOwnerID);

            networkAddressEntity.TimeToLive.Should().Be(networkAddressEntity2.TimeToLive);
            networkAddressEntity.Score.Should().Be(networkAddressEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            networkAddressEntity2.CreatedUtc = networkAddressEntity.CreatedUtc;
            networkAddressEntity2.ModifiedUtc = networkAddressEntity.ModifiedUtc;
            networkAddressEntity.Should().BeEquivalentTo(networkAddressEntity2);

            networkAddressEntity.Entity.Should().Be(networkAddressEntity2.Entity);
        }

        [Theory]
        [MemberData(nameof(TestNetworkAddresses))]
        public void NetworkAddressEntityUnit_Same(IPAddress networkAddress, int networkAddressPrefixLength, CoreNetworkAddressEntityType networkAddressEntityType, CoreIPAddressScore expectedScore)
        {
            var networkAddressEntity = new CoreNetworkAddressEntity(networkAddress, networkAddressPrefixLength);

            this.ValidateAndOutputEntity<CoreNetworkAddressEntity>(networkAddressEntity, CoreEntityType.NetworkAddressV1, (ulong)expectedScore, CoreNetworkAddressEntity.CreateLookupKey(networkAddressEntity.NetworkAddress));
            networkAddressEntity.NetworkAddressEntityType.Should().Be(networkAddressEntityType);
            networkAddressEntity.NetworkAddress.Should().Be(networkAddress);
            networkAddressEntity.NetworkAddressPrefixLength.Should().Be(CoreNetworkAddressEntity.CalculateNetworkAddressPrefixLength(networkAddress, networkAddressPrefixLength));
            networkAddressEntity.IPAddressScore.Should().Be(expectedScore);

            var networkAddressEntity2 = new CoreNetworkAddressEntity(networkAddress, networkAddressPrefixLength);
            networkAddressEntity.EntityID.Should().Be(networkAddressEntity2.EntityID);
            networkAddressEntity.DisplayName.Should().Be(networkAddressEntity2.DisplayName);
            networkAddressEntity.EntityType.Should().Be(networkAddressEntity2.EntityType);
            networkAddressEntity.EntityOwnerID.Should().Be(networkAddressEntity2.EntityOwnerID);

            networkAddressEntity.TimeToLive.Should().Be(networkAddressEntity2.TimeToLive);
            networkAddressEntity.Score.Should().Be(networkAddressEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            networkAddressEntity2.CreatedUtc = networkAddressEntity.CreatedUtc;
            networkAddressEntity2.ModifiedUtc = networkAddressEntity.ModifiedUtc;
            networkAddressEntity.Should().BeEquivalentTo(networkAddressEntity2);

            networkAddressEntity.Entity.Should().Be(networkAddressEntity2.Entity);
        }
    }
}
