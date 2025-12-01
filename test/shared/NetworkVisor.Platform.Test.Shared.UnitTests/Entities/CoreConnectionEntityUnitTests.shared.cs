// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreConnectionEntityUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Connections;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.UnitTests.Entities
{
    /// <summary>
    /// Class CoreConnectionEntityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreConnectionEntityUnitTests))]
    public class CoreConnectionEntityUnitTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreConnectionEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreConnectionEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void ConnectionEntityUnit_Default()
        {
            var connectionEntity = new CoreConnectionEntity();
            this.ValidateAndOutputEntity<CoreConnectionEntity>(connectionEntity, CoreEntityType.ConnectionV1, CoreEntityConstants.DefaultConnectionWeight, CoreConnectionEntity.CreateLookupKey(CoreConnectionEntityType.UnknownConnection, Guid.Empty, Guid.Empty));
            connectionEntity.LookupKey.Should().Be(CoreConnectionEntity.CreateLookupKey(CoreConnectionEntityType.UnknownConnection, CoreEntityConstants.NoneEntityID, CoreEntityConstants.NoneEntityID));
        }

        [Fact]
        public void ConnectionEntityUnit_Default_Same()
        {
            var connectionEntity = new CoreConnectionEntity();
            this.ValidateAndOutputEntity<CoreConnectionEntity>(connectionEntity, CoreEntityType.ConnectionV1, CoreEntityConstants.DefaultConnectionWeight, CoreConnectionEntity.CreateLookupKey(CoreConnectionEntityType.UnknownConnection, Guid.Empty, Guid.Empty));
            var connectionEntity2 = new CoreConnectionEntity();
            connectionEntity.EntityID.Should().Be(connectionEntity2.EntityID);
            connectionEntity.DisplayName.Should().Be(connectionEntity2.DisplayName);
            connectionEntity.EntityType.Should().Be(connectionEntity2.EntityType);
            connectionEntity.EntityOwnerID.Should().Be(connectionEntity2.EntityOwnerID);
            connectionEntity.FromEntityID.Should().Be(connectionEntity.FromEntityID);
            connectionEntity.ToEntityID.Should().Be(connectionEntity.ToEntityID);
            connectionEntity.TimeToLive.Should().Be(connectionEntity2.TimeToLive);
            connectionEntity.Score.Should().Be(connectionEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            connectionEntity2.CreatedUtc = connectionEntity.CreatedUtc;
            connectionEntity2.ModifiedUtc = connectionEntity.ModifiedUtc;
            connectionEntity.Should().BeEquivalentTo(connectionEntity2);

            connectionEntity.Entity.Should().Be(connectionEntity2.Entity);
        }

        [Fact]
        public void ConnectionEntityUnit_Same()
        {
            Guid fromEntityId = Guid.NewGuid();
            Guid toEntityId = Guid.NewGuid();

            var connectionEntity = new CoreConnectionEntity(CoreConnectionEntityType.GenericConnection, fromEntityId, toEntityId);
            this.ValidateAndOutputEntity<CoreConnectionEntity>(connectionEntity, CoreEntityType.ConnectionV1, CoreEntityConstants.DefaultConnectionWeight, CoreConnectionEntity.CreateLookupKey(CoreConnectionEntityType.GenericConnection, fromEntityId, toEntityId));
            var connectionEntity2 = new CoreConnectionEntity(CoreConnectionEntityType.GenericConnection, fromEntityId, toEntityId);
            connectionEntity.EntityID.Should().Be(connectionEntity2.EntityID);
            connectionEntity.DisplayName.Should().Be(connectionEntity2.DisplayName);
            connectionEntity.EntityType.Should().Be(connectionEntity2.EntityType);
            connectionEntity.EntityOwnerID.Should().Be(connectionEntity2.EntityOwnerID);
            connectionEntity.FromEntityID.Should().Be(connectionEntity.FromEntityID);
            connectionEntity.ToEntityID.Should().Be(connectionEntity.ToEntityID);
            connectionEntity.TimeToLive.Should().Be(connectionEntity2.TimeToLive);
            connectionEntity.Score.Should().Be(connectionEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            connectionEntity2.CreatedUtc = connectionEntity.CreatedUtc;
            connectionEntity2.ModifiedUtc = connectionEntity.ModifiedUtc;
            connectionEntity.Should().BeEquivalentTo(connectionEntity2);

            connectionEntity.Entity.Should().Be(connectionEntity2.Entity);
        }
    }
}
