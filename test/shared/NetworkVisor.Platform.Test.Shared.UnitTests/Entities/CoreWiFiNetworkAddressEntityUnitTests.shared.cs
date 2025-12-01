// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreWiFiNetworkAddressEntityUnitTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Entities.Connections;
using NetworkVisor.Core.Entities.Connections.WiFi;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Entities.Networks.Base;
using NetworkVisor.Core.Entities.Networks.WiFi;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Networking.WiFi;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Serialization;
using NetworkVisor.Platform.Test.TestCase;
using NetworkVisor.Platform.Test.TestEntities;
using Xunit;
using Xunit.Sdk;

namespace NetworkVisor.Platform.Test.Messaging.Shared.UnitTests.Entities
{
    /// <summary>
    /// Class CoreWiFiNetworkEntityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreWiFiNetworkAddressEntityUnitTests))]
    public class CoreWiFiNetworkAddressEntityUnitTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreWiFiNetworkAddressEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreWiFiNetworkAddressEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void WiFiNetworkAddressEntityUnit_Default()
        {
            var networkAddressEntity = new CoreWiFiNetworkAddressEntity();
            this.ValidateAndOutputEntity<CoreWiFiNetworkAddressEntity>(networkAddressEntity, CoreEntityType.WiFiNetworkAddressV1, CoreWiFiNetworkAddressEntity.ScoreFromRssi(CoreWiFiNetworkAddressConstants.NoSignalRSSI), CoreWiFiNetworkAddressEntity.CreateLookupKeyFromBssid(networkAddressEntity.BSSID));

            // WiFiNetworkEntity specific validations
            networkAddressEntity.SSID.Should().Be(CoreWiFiNetworkAddressConstants.RestrictedSSID);
        }

        [Fact]
        public void WiFiNetworkAddressEntityUnit_Default_Same()
        {
            var networkAddressEntity = new CoreWiFiNetworkAddressEntity();
            this.ValidateAndOutputEntity<CoreWiFiNetworkAddressEntity>(networkAddressEntity, CoreEntityType.WiFiNetworkAddressV1, CoreWiFiNetworkAddressEntity.ScoreFromRssi(CoreWiFiNetworkAddressConstants.NoSignalRSSI), CoreWiFiNetworkAddressEntity.CreateLookupKeyFromBssid(networkAddressEntity.BSSID));
            var networkAddressEntity2 = new CoreWiFiNetworkAddressEntity(networkAddressEntity, networkAddressEntity.BSSID, networkAddressEntity.SSID, networkAddressEntity.RSSI, networkAddressEntity.LastSeenUtc);

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
            networkAddressEntity.LastSeenUtc.Should().Be(networkAddressEntity2.LastSeenUtc);
            networkAddressEntity.SSID.Should().Be(networkAddressEntity2.SSID);
            networkAddressEntity.BSSID.Should().Be(networkAddressEntity2.BSSID);
            networkAddressEntity.RSSI.Should().Be(networkAddressEntity2.RSSI);
            networkAddressEntity.WiFiSignalStrength.Should().Be(networkAddressEntity2.WiFiSignalStrength);
            networkAddressEntity.LastSeenUtc.Should().Be(networkAddressEntity2.LastSeenUtc);
            networkAddressEntity.CreatedUtc.Should().Be(networkAddressEntity2.CreatedUtc);
            networkAddressEntity.ModifiedUtc.Should().Be(networkAddressEntity2.ModifiedUtc);
        }

        [Theory]
        [InlineData(CoreWiFiNetworkAddressConstants.NoSignalRSSI, CoreWiFiSignalStrength.NoSignal)]
        [InlineData(0, CoreWiFiSignalStrength.NoSignal)]
        [InlineData(-31, CoreWiFiSignalStrength.Excellent)]
        [InlineData(-39, CoreWiFiSignalStrength.Excellent)]
        [InlineData(-50, CoreWiFiSignalStrength.Excellent)]
        [InlineData(-51, CoreWiFiSignalStrength.Good)]
        [InlineData(-79, CoreWiFiSignalStrength.Weak)]
        [InlineData(-87, CoreWiFiSignalStrength.VeryWeak)]
        [InlineData(-90, CoreWiFiSignalStrength.VeryWeak)]
        [InlineData(-91, CoreWiFiSignalStrength.NoSignal)]
        public void WiFiNetworkAddressEntityUnit_ScoreFromRssi(int rssi, CoreWiFiSignalStrength wiFiSignalStrength)
        {
            var score = CoreWiFiNetworkAddressEntity.ScoreFromRssi(rssi);
            var networkAddressEntity = new CoreWiFiNetworkAddressEntity(CoreWiFiNetworkAddressConstants.RestrictedBSSID, CoreWiFiNetworkAddressConstants.RestrictedSSID, rssi, DateTimeOffset.UtcNow);
            this.ValidateAndOutputEntity<CoreWiFiNetworkAddressEntity>(networkAddressEntity, CoreEntityType.WiFiNetworkAddressV1, score, CoreWiFiNetworkAddressEntity.CreateLookupKeyFromBssid(networkAddressEntity.BSSID));
            networkAddressEntity.WiFiSignalStrength.Should().Be(wiFiSignalStrength);
        }

        [Theory]
        [InlineData(null, "96dd196e-7708-6256-b6b6-42bbde6dddea")]
        [InlineData("", "96dd196e-7708-6256-b6b6-42bbde6dddea")]
        [InlineData("42:3c:04:71:f0:0e", "507f2095-73f0-d75f-9c45-a5c86912fa83")]
        public void WiFiNetworkAddressEntityUnit_CreateEntityIDFromBssid(string? bssidString, string entityIDString)
        {
            PhysicalAddress bssid = PhysicalAddressExtensions.NormalizedParse(bssidString);
            Guid entityIDExpected = Guid.Parse(entityIDString);
            Guid entityID = CoreWiFiNetworkAddressEntity.CreateEntityIDFromBssid(bssid);
            entityID.Should().Be(entityIDExpected);

            var networkAddressEntity = new CoreWiFiNetworkAddressEntity(bssid, CoreWiFiNetworkAddressConstants.RestrictedSSID, -30, DateTimeOffset.UtcNow);
            networkAddressEntity.EntityID.Should().Be(entityIDExpected);
            this.ValidateAndOutputEntity<CoreWiFiNetworkAddressEntity>(networkAddressEntity, CoreEntityType.WiFiNetworkAddressV1, CoreWiFiNetworkAddressEntity.ScoreFromRssi(-30), CoreWiFiNetworkAddressEntity.CreateLookupKeyFromBssid(networkAddressEntity.BSSID));

            var networkAddressEntity2 = new CoreWiFiNetworkAddressEntity(bssid, CoreWiFiNetworkAddressConstants.RestrictedSSID, -30, DateTimeOffset.UtcNow);
            networkAddressEntity2.EntityID.Should().Be(entityIDExpected);

            // Fix up the CreatedUtc and ModifiedUtc to be the same
            networkAddressEntity2.CreatedUtc = networkAddressEntity.CreatedUtc;
            networkAddressEntity2.ModifiedUtc = networkAddressEntity.ModifiedUtc;
            networkAddressEntity2.LastSeenUtc = networkAddressEntity.LastSeenUtc;

            networkAddressEntity.Should().BeEquivalentTo(networkAddressEntity2);
        }
    }
}
