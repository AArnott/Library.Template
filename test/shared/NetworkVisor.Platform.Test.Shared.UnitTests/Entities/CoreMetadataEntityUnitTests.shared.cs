// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreMetadataEntityUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using FluentAssertions;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Entities.Metadata;
using NetworkVisor.Core.Entities.Metadata.Base;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.UnitTests.Entities
{
    /// <summary>
    /// Class CoreMetadataEntityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreMetadataEntityUnitTests))]
    public class CoreMetadataEntityUnitTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreMetadataEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreMetadataEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreMetadataEntityUnit_Default()
        {
            var metadataEntity = new CoreMetadataEntity();
            this.ValidateAndOutputEntity<CoreMetadataEntity>(metadataEntity, CoreEntityType.MetadataV1, CoreEntityConstants.DefaultEntityScore, CoreMetadataEntityType.Unknown.GetDescription());
        }

        [Fact]
        public void CoreMetadataEntityUnit_Default_Same()
        {
            var metadataEntity = new CoreMetadataEntity();
            this.ValidateAndOutputEntity<CoreMetadataEntity>(metadataEntity, CoreEntityType.MetadataV1, CoreEntityConstants.DefaultEntityScore, CoreMetadataEntityType.Unknown.GetDescription());
            var metadataEntity2 = new CoreMetadataEntity();

            _ = metadataEntity.EntityID.Should().Be(metadataEntity2.EntityID);
            _ = metadataEntity.DisplayName.Should().Be(metadataEntity2.DisplayName);
            _ = metadataEntity.EntityType.Should().Be(metadataEntity2.EntityType);
            _ = metadataEntity.EntityOwnerID.Should().Be(metadataEntity2.EntityOwnerID);
            _ = metadataEntity.MetadataEntityType.Should().Be(metadataEntity.MetadataEntityType);
            _ = metadataEntity.TimeToLive.Should().Be(metadataEntity2.TimeToLive);
            _ = metadataEntity.Score.Should().Be(metadataEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            metadataEntity2.CreatedUtc = metadataEntity.CreatedUtc;
            metadataEntity2.ModifiedUtc = metadataEntity.ModifiedUtc;
            _ = metadataEntity.Should().BeEquivalentTo(metadataEntity2);

            _ = metadataEntity.Entity.Should().Be(metadataEntity2.Entity);
        }

        [Fact]
        public void CoreMetadataEntityUnit_Same()
        {
            var metadataEntity = new CoreMetadataEntity(CoreMetadataEntityType.Unknown);
            this.ValidateAndOutputEntity<CoreMetadataEntity>(metadataEntity, CoreEntityType.MetadataV1, CoreEntityConstants.DefaultEntityScore, CoreMetadataEntityType.Unknown.GetDescription());
            var metadataEntity2 = new CoreMetadataEntity(CoreMetadataEntityType.Unknown);
            _ = metadataEntity.EntityID.Should().Be(metadataEntity2.EntityID);
            _ = metadataEntity.DisplayName.Should().Be(metadataEntity2.DisplayName);
            _ = metadataEntity.EntityType.Should().Be(metadataEntity2.EntityType);
            _ = metadataEntity.EntityOwnerID.Should().Be(metadataEntity2.EntityOwnerID);
            _ = metadataEntity.MetadataEntityType.Should().Be(metadataEntity.MetadataEntityType);
            _ = metadataEntity.TimeToLive.Should().Be(metadataEntity2.TimeToLive);
            _ = metadataEntity.Score.Should().Be(metadataEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            metadataEntity2.CreatedUtc = metadataEntity.CreatedUtc;
            metadataEntity2.ModifiedUtc = metadataEntity.ModifiedUtc;
            _ = metadataEntity.Should().BeEquivalentTo(metadataEntity2);

            _ = metadataEntity.Entity.Should().Be(metadataEntity2.Entity);
        }
    }
}
