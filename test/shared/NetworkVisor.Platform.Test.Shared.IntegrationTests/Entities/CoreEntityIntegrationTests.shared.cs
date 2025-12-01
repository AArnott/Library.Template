// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreEntityIntegrationTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Entities
{
    /// <summary>
    /// Class CoreEntityIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreEntityIntegrationTests))]
    public class CoreEntityIntegrationTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEntityIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreEntityIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task EntityIntegration_InsertEntityAsync()
        {
            var entity = new CoreEntity();
            (await this.TestEntityDatabase.InsertEntityAsync(entity)).Should().Be(1);
            this.ValidateAndOutputEntity(entity, CoreEntityType.EntityV1, CoreEntityConstants.DefaultEntityScore);
        }

        [Fact]
        public async Task EntityIntegration_InsertOrReplaceEntityAsync()
        {
            var entity = new CoreEntity();
            await this.TestEntityDatabase.InsertEntityAsync(entity);
            entity.Score = 10;
            (await this.TestEntityDatabase.InsertOrReplaceEntityAsync(entity)).Should().Be(1);
            this.ValidateAndOutputEntity(entity, CoreEntityType.EntityV1, 10);
        }

        [Fact]
        public async Task EntityIntegration_GetEntityAsync()
        {
            var entity = new CoreEntity();
            await this.TestEntityDatabase.InsertEntityAsync(entity);
            CoreEntity? retrievedEntity =
                await this.TestEntityDatabase.GetEntityAsync(entity.EntityID, CoreEntityConstants.DefaultSnapshotID);
            retrievedEntity.Should().NotBeNull();
            retrievedEntity!.EntityID.Should().Be(entity.EntityID);
        }

        [Fact]
        public async Task EntityIntegration_DeleteEntityAsync()
        {
            var entity = new CoreEntity();
            await this.TestEntityDatabase.InsertEntityAsync(entity);
            (await this.TestEntityDatabase.DeleteEntityAsync(entity, CoreEntityConstants.DefaultSnapshotID)).Should()
                .Be(1);
            CoreEntity? retrievedEntity =
                await this.TestEntityDatabase.GetEntityAsync(entity.EntityID, CoreEntityConstants.DefaultSnapshotID);
            retrievedEntity.Should().BeNull();
        }
    }
}
