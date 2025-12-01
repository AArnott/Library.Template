// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreNetworkAddressEntityIntegrationTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Extensions;
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

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Entities
{
    /// <summary>
    /// Class CoreNetworkAddressEntityIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkAddressEntityIntegrationTests))]
    public class CoreNetworkAddressEntityIntegrationTests : CoreEntityTestCaseBase
    {
        public static (IPAddress, int, CoreNetworkAddressEntityType, CoreIPAddressScore)[] TestNetworkAddresses = TestNetworkAddresses = CoreTestEntityConstants.TestNetworkAddresses;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkAddressEntityIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreNetworkAddressEntityIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [MemberData(nameof(TestNetworkAddresses))]
        public async Task NetworkAddressEntityIntegration_InsertEntity(IPAddress networkAddress, int networkAddressPrefixLength, CoreNetworkAddressEntityType networkAddressEntityType, CoreIPAddressScore expectedScore)
        {
            var networkAddressEntity = new CoreNetworkAddressEntity(networkAddress, networkAddressPrefixLength);
            networkAddressEntity.NetworkAddress.Should().Be(networkAddress);
            networkAddressEntity.NetworkAddressPrefixLength.Should().Be(CoreNetworkAddressEntity.CalculateNetworkAddressPrefixLength(networkAddress, networkAddressPrefixLength));
            networkAddressEntity.NetworkAddressEntityType.Should().Be(networkAddressEntityType);
            networkAddressEntity.IPAddressScore.Should().Be(expectedScore);
            networkAddressEntity.Score.Should().Be((ulong)expectedScore);

            this.ValidateAndOutputEntity<CoreNetworkAddressEntity>(networkAddressEntity, CoreEntityType.NetworkAddressV1, (ulong)expectedScore, CoreNetworkAddressEntity.CreateLookupKey(networkAddressEntity.NetworkAddress));
            networkAddressEntity.NetworkAddressEntityType.Should().Be(networkAddressEntityType);
            networkAddressEntity.NetworkAddress.Should().Be(networkAddress);
            networkAddressEntity.NetworkAddressPrefixLength.Should().Be(CoreNetworkAddressEntity.CalculateNetworkAddressPrefixLength(networkAddress, networkAddressPrefixLength));

            (await this.TestEntityDatabase.InsertEntityAsync(networkAddressEntity)).Should().Be(1);
            CoreNetworkAddressEntity? databaseNetworkAddressEntity = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreNetworkAddressEntity>(networkAddressEntity.EntityID, CoreEntityConstants.DefaultSnapshotID);

            databaseNetworkAddressEntity.Should().NotBeNull();
            databaseNetworkAddressEntity.Should().BeEquivalentTo(networkAddressEntity);
            this.TestOutputHelper.WriteLine($"\n{"Database Entity".CenterTitle()}\n");
            this.ValidateAndOutputEntity<CoreNetworkAddressEntity>(databaseNetworkAddressEntity, CoreEntityType.NetworkAddressV1, (ulong)expectedScore, CoreNetworkAddressEntity.CreateLookupKey(networkAddressEntity.NetworkAddress));
        }

        [Theory]
        [MemberData(nameof(TestNetworkAddresses))]
        public async Task NetworkAddressEntityIntegration_InsertEntityTwice(IPAddress networkAddress, int networkAddressPrefixLength, CoreNetworkAddressEntityType networkAddressEntityType, CoreIPAddressScore expectedScore)
        {
            var networkAddressEntity = new CoreNetworkAddressEntity(networkAddress, networkAddressPrefixLength);
            networkAddressEntity.NetworkAddress.Should().Be(networkAddress);
            networkAddressEntity.NetworkAddressPrefixLength.Should().Be(CoreNetworkAddressEntity.CalculateNetworkAddressPrefixLength(networkAddress, networkAddressPrefixLength));
            networkAddressEntity.NetworkAddressEntityType.Should().Be(networkAddressEntityType);
            networkAddressEntity.IPAddressScore.Should().Be(expectedScore);
            networkAddressEntity.Score.Should().Be((ulong)expectedScore);

            (await this.TestEntityDatabase.InsertEntityAsync(networkAddressEntity)).Should().Be(1);
            CoreNetworkAddressEntity? databaseNetworkAddressEntity = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreNetworkAddressEntity>(networkAddressEntity.EntityID, CoreEntityConstants.DefaultSnapshotID);
            databaseNetworkAddressEntity.Should().NotBeNull();
            databaseNetworkAddressEntity.NetworkAddressEntityType.Should().Be(networkAddressEntityType);
            databaseNetworkAddressEntity.NetworkAddress.Should().Be(networkAddress);
            databaseNetworkAddressEntity.NetworkAddressPrefixLength.Should().Be(CoreNetworkAddressEntity.CalculateNetworkAddressPrefixLength(networkAddress, networkAddressPrefixLength));
            databaseNetworkAddressEntity.Should().BeEquivalentTo(networkAddressEntity);

            this.TestOutputHelper.WriteLine($"\n{"Inserted Entity".CenterTitle()}\n");
            this.ValidateAndOutputEntity<CoreNetworkAddressEntity>(databaseNetworkAddressEntity, CoreEntityType.NetworkAddressV1, (ulong)expectedScore, CoreNetworkAddressEntity.CreateLookupKey(networkAddressEntity.NetworkAddress));

            // Validate that we cannot insert the same entity again
            (await this.TestEntityDatabase.InsertEntityAsync(networkAddressEntity)).Should().Be(0);
        }

        [Theory]
        [MemberData(nameof(TestNetworkAddresses))]
        public async Task NetworkAddressEntityIntegration_InsertOrReplaceTwice(IPAddress networkAddress, int networkAddressPrefixLength, CoreNetworkAddressEntityType networkAddressEntityType, CoreIPAddressScore expectedScore)
        {
            var networkAddressEntity = new CoreNetworkAddressEntity(networkAddress, networkAddressPrefixLength);
            networkAddressEntity.NetworkAddress.Should().Be(networkAddress);
            networkAddressEntity.NetworkAddressPrefixLength.Should().Be(CoreNetworkAddressEntity.CalculateNetworkAddressPrefixLength(networkAddress, networkAddressPrefixLength));
            networkAddressEntity.NetworkAddressEntityType.Should().Be(networkAddressEntityType);
            networkAddressEntity.IPAddressScore.Should().Be(expectedScore);
            networkAddressEntity.Score.Should().Be((ulong)expectedScore);

            (await this.TestEntityDatabase.InsertOrReplaceEntityAsync(networkAddressEntity)).Should().Be(1);
            CoreNetworkAddressEntity? databaseNetworkAddressEntity = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreNetworkAddressEntity>(networkAddressEntity.EntityID, CoreEntityConstants.DefaultSnapshotID);
            databaseNetworkAddressEntity.Should().NotBeNull();
            databaseNetworkAddressEntity.NetworkAddressEntityType.Should().Be(networkAddressEntityType);
            databaseNetworkAddressEntity.NetworkAddress.Should().Be(networkAddress);
            databaseNetworkAddressEntity.NetworkAddressPrefixLength.Should().Be(CoreNetworkAddressEntity.CalculateNetworkAddressPrefixLength(networkAddress, networkAddressPrefixLength));
            databaseNetworkAddressEntity.Should().BeEquivalentTo(networkAddressEntity);

            this.TestOutputHelper.WriteLine($"\n{"Inserted Entity".CenterTitle()}\n");
            this.ValidateAndOutputEntity<CoreNetworkAddressEntity>(databaseNetworkAddressEntity, CoreEntityType.NetworkAddressV1, (ulong)expectedScore, CoreNetworkAddressEntity.CreateLookupKey(networkAddressEntity.NetworkAddress));

            // Validate that we can insert the same entity again
            (await this.TestEntityDatabase.InsertOrReplaceEntityAsync(networkAddressEntity)).Should().Be(1);
            databaseNetworkAddressEntity = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreNetworkAddressEntity>(networkAddressEntity.EntityID, CoreEntityConstants.DefaultSnapshotID);
            databaseNetworkAddressEntity.Should().BeEquivalentTo(networkAddressEntity);
        }
    }
}
