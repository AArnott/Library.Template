// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreHostEntityUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using System.Net;
using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Database;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Entities.Hosts;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using NetworkVisor.Platform.Test.TestEntities;
using Paramore.Brighter.ServiceActivator;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.UnitTests.Entities
{
    /// <summary>
    /// Class CoreHostEntityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreHostEntityUnitTests))]
    public class CoreHostEntityUnitTests : CoreEntityTestCaseBase
    {
        public static (CoreHostEntityType, string, Guid, CoreHostEntityScore)[] TestHostEntities = CoreTestEntityConstants.TestHostEntities;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreHostEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreHostEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreHostEntityUnitTests_Default()
        {
            CoreHostEntity hostEntity = new CoreHostEntity();
            this.ValidateAndOutputEntity<CoreHostEntity>(hostEntity, CoreEntityType.HostV1, (ulong)CoreHostEntityScore.Default);

            // Host specific validations
            hostEntity.HostName.Should().Be(hostEntity.LookupKey);
            hostEntity.HostEntityType.Should().Be(CoreHostEntityType.GenericHost);
            hostEntity.Aliases.Should().BeEmpty();
        }

        [Theory]
        [InlineData(CoreHostEntityType.UnknownHost)]
        [InlineData(CoreHostEntityType.GenericHost)]
        [InlineData(CoreHostEntityType.DnsHost)]
        [InlineData(CoreHostEntityType.MDnsHost)]
        [InlineData(CoreHostEntityType.PingHost)]
        public void CoreHostEntityUnitTests_HostEntityType(CoreHostEntityType hostEntityType)
        {
            CoreHostEntity hostEntity = new CoreHostEntity(hostEntityType);
            this.ValidateAndOutputEntity<CoreHostEntity>(hostEntity, CoreEntityType.HostV1, (ulong)CoreHostEntityScore.Default);

            // Host specific validations
            hostEntity.HostName.Should().Be(hostEntity.LookupKey);
            hostEntity.HostEntityType.Should().Be(hostEntityType);
            hostEntity.Aliases.Should().BeEmpty();
        }

        [Theory]
        [InlineData(CoreHostEntityType.UnknownHost, CoreEntityConstants.UnknownHostName)]
        [InlineData(CoreHostEntityType.GenericHost, "localhost")]
        [InlineData(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer)]
        [InlineData(CoreHostEntityType.MDnsHost, "_airplay._tcp.local.")]
        [InlineData(CoreHostEntityType.PingHost, CoreIPAddressExtensions.StringGooglePublicDnsServer)]
        public void CoreHostEntityUnitTests_HostName(CoreHostEntityType hostEntityType, string hostName)
        {
            CoreHostEntity hostEntity = new CoreHostEntity(hostEntityType, hostName);
            this.ValidateAndOutputEntity<CoreHostEntity>(hostEntity, CoreEntityType.HostV1, (ulong)CoreHostEntityScore.Default, hostName);

            // Host specific validations
            hostEntity.HostName.Should().Be(hostName);
            hostEntity.HostName.Should().Be(hostEntity.LookupKey);
            hostEntity.HostEntityType.Should().Be(hostEntityType);
            hostEntity.Aliases.Should().BeEmpty();
        }

        [Fact]
        public void CoreHostEntityUnitTests_Default_Same()
        {
            var hostEntity = new CoreHostEntity();
            this.ValidateAndOutputEntity<CoreHostEntity>(hostEntity, CoreEntityType.HostV1, CoreEntityConstants.DefaultConnectionWeight);
            var hostEntity2 = new CoreHostEntity();
            hostEntity.EntityID.Should().Be(hostEntity2.EntityID);
            hostEntity.DisplayName.Should().Be(hostEntity2.DisplayName);
            hostEntity.EntityType.Should().Be(hostEntity2.EntityType);
            hostEntity.EntityOwnerID.Should().Be(hostEntity2.EntityOwnerID);

            hostEntity.TimeToLive.Should().Be(hostEntity2.TimeToLive);
            hostEntity.Score.Should().Be(hostEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            hostEntity2.CreatedUtc = hostEntity.CreatedUtc;
            hostEntity2.ModifiedUtc = hostEntity.ModifiedUtc;
            hostEntity.Should().BeEquivalentTo(hostEntity2);

            hostEntity.Entity.Should().Be(hostEntity2.Entity);
        }

        [Theory]
        [MemberData(nameof(TestHostEntities))]
        public void CoreHostEntityUnitTests_Same(CoreHostEntityType hostEntityType, string hostName, Guid expectedEntityId, CoreHostEntityScore expectedScore)
        {
            var hostEntity = new CoreHostEntity(hostEntityType, hostName);

            this.ValidateAndOutputEntity<CoreHostEntity>(hostEntity, CoreEntityType.HostV1, (ulong)expectedScore, CoreHostEntity.CreateLookupKey(hostName));
            hostEntity.EntityID.Should().Be(expectedEntityId);
            hostEntity.Score.Should().Be((ulong)expectedScore);

            var hostEntity2 = new CoreHostEntity(hostEntityType, hostName);
            hostEntity.EntityID.Should().Be(hostEntity2.EntityID);
            hostEntity.DisplayName.Should().Be(hostEntity2.DisplayName);
            hostEntity.EntityType.Should().Be(hostEntity2.EntityType);
            hostEntity.EntityOwnerID.Should().Be(hostEntity2.EntityOwnerID);

            hostEntity.TimeToLive.Should().Be(hostEntity2.TimeToLive);
            hostEntity.Score.Should().Be(hostEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            hostEntity2.CreatedUtc = hostEntity.CreatedUtc;
            hostEntity2.ModifiedUtc = hostEntity.ModifiedUtc;
            hostEntity.Should().BeEquivalentTo(hostEntity2);

            hostEntity.Entity.Should().Be(hostEntity2.Entity);
        }
    }
}
