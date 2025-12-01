// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreEntityDatabaseIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Connections;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Database;
using NetworkVisor.Core.Entities.Hosts;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Entities.Database
{
    /// <summary>
    /// Class CoreEntityDatabaseIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreEntityDatabaseIntegrationTests))]
    public class CoreEntityDatabaseIntegrationTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEntityDatabaseIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreEntityDatabaseIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void EntityDatabaseIntegration_EntityDatabase()
        {
            _ = this.TestEntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreEntityDatabase>();

            // Ensure the database is initialized and the CoreSQLiteAsyncConnection is set up correctly
            _ = this.TestEntityDatabase.SQLiteAsyncConnection.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreSQLiteAsyncConnection>();
            _ = this.TestFileSystem.FileExists(this.TemporaryEntityDatabasePath).Should().BeTrue();
        }

        [Fact]
        public async Task EntityDatabaseIntegration_Table_Initial()
        {
            _ = this.TestEntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreEntityDatabase>();
            _ = (await this.TestEntityDatabase.SQLiteAsyncConnection.Table<CoreEntity>().CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task EntityDatabaseIntegration_Empty_DeleteAllAsync()
        {
            _ = this.TestEntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreEntityDatabase>();
            _ = (await this.TestEntityDatabase.SQLiteAsyncConnection.DeleteAllAsync<CoreEntity>()).Should().Be(0);
        }

        [Fact]
        public void EntityDatabaseIntegration_Entity()
        {
            _ = this.TestEntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreEntityDatabase>();

            var entity = new CoreEntity();
            _ = entity.LookupKey.Should().Be(CoreEntityConstants.CreateLookupKeyFromEntityID(entity.EntityID));
            _ = entity.Entity.Should().Be(CoreEntityBase.CalculateEntityJson(entity));

            this.OutputEntity(entity!);
        }

        [Fact]
        public async Task EntityDatabaseIntegration_InsertAsync()
        {
            _ = this.TestEntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreEntityDatabase>();

            var entity = new CoreEntity();
            _ = entity.LookupKey.Should().Be(CoreEntityConstants.CreateLookupKeyFromEntityID(entity.EntityID));
            _ = entity.Entity.Should().Be(CoreEntityBase.CalculateEntityJson(entity));

            _ = (await this.TestEntityDatabase.SQLiteAsyncConnection.InsertAsync(entity)).Should().NotBe(0);
            _ = (await this.TestEntityDatabase.SQLiteAsyncConnection.Table<CoreEntity>().CountAsync()).Should().Be(1);

            CoreEntity? insertedEntity = await this.TestEntityDatabase.SQLiteAsyncConnection.Table<CoreEntity>().FirstOrDefaultAsync();
            _ = insertedEntity.Should().NotBeNull().And.Subject.Should().BeOfType<CoreEntity>();

            this.OutputEntity(insertedEntity!);

            _ = insertedEntity.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public void DatabaseInitialization_DatabaseExists_ShouldBeTrue()
        {
            // Arrange & Act
            ICoreEntityDatabase database = this.TestEntityDatabase;

            // Assert
            _ = database.Should().NotBeNull();
            _ = database.Should().BeAssignableTo<ICoreEntityDatabase>();
            _ = database.SQLiteAsyncConnection.Should().NotBeNull();
            _ = this.TestFileSystem.FileExists(this.TemporaryEntityDatabasePath).Should().BeTrue();
        }

        [Fact]
        public async Task DatabaseInitialization_TablesExist_ShouldBeTrue()
        {
            // Arrange & Act
            Core.Database.Providers.SQLite.Tables.AsyncTableQuery<CoreEntity> entityTable = this.TestEntityDatabase.EntityTable;
            Core.Database.Providers.SQLite.Tables.AsyncTableQuery<CoreConnectionEntity> connectionTable = this.TestEntityDatabase.ConnectionEntityTable;

            // Assert
            _ = entityTable.Should().NotBeNull();
            _ = connectionTable.Should().NotBeNull();

            // Verify tables are empty initially
            _ = (await entityTable.CountAsync()).Should().Be(0);
            _ = (await connectionTable.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task InsertEntityAsync_ValidEntity_ShouldReturnSuccess()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-1");
            this.ValidateAndOutputEntity(entity, CoreEntityType.EntityV1, CoreEntityConstants.DefaultEntityScore, "test-entity-1");

            // Act
            int result = await this.TestEntityDatabase.InsertEntityAsync(entity);

            // Assert
            _ = result.Should().BeGreaterThan(0);
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            // Verify entity was inserted correctly
            CoreEntity? insertedEntity = await this.TestEntityDatabase.EntityTable.FirstOrDefaultAsync();
            _ = insertedEntity.Should().NotBeNull();
            _ = insertedEntity!.EntityID.Should().Be(entity.EntityID);
            _ = insertedEntity.LookupKey.Should().Be(entity.LookupKey);
        }

        [Fact]
        public async Task InsertEntityAsync_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            CoreEntity? entity = null;

            // Act & Assert
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.TestEntityDatabase.InsertEntityAsync(entity!));
        }

        [Fact]
        public async Task InsertEntityAsync_MultipleEntities_ShouldInsertAll()
        {
            // Arrange
            var entities = new List<CoreEntity>
            {
                new(CoreEntityType.EntityV1, "test-entity-1"),
                new(CoreEntityType.EntityV1, "test-entity-2"),
                new(CoreEntityType.EntityV1, "test-entity-3"),
            };

            // Act
            foreach (CoreEntity entity in entities)
            {
                int result = await this.TestEntityDatabase.InsertEntityAsync(entity);
                _ = result.Should().BeGreaterThan(0);
            }

            // Assert
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(3);
        }

        [Fact]
        public async Task InsertOrReplaceEntityAsync_NewEntity_ShouldInsert()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-replace");
            this.ValidateAndOutputEntity(entity, CoreEntityType.EntityV1, CoreEntityConstants.DefaultEntityScore, "test-entity-replace");

            // Act
            int result = await this.TestEntityDatabase.InsertOrReplaceEntityAsync(entity);

            // Assert
            _ = result.Should().BeGreaterThan(0);
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task InsertOrReplaceEntityAsync_ExistingEntity_ShouldReplace()
        {
            // Arrange
            var originalEntity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-replace");
            _ = await this.TestEntityDatabase.InsertEntityAsync(originalEntity);

            // Create a new entity with same ID but different properties
            var updatedEntity = new CoreEntity(originalEntity)
            {
                DisplayName = "Updated Entity",
                Score = 100,
            };

            // Act
            int result = await this.TestEntityDatabase.InsertOrReplaceEntityAsync(updatedEntity);

            // Assert
            _ = result.Should().BeGreaterThan(0);
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            // Verify the entity was updated
            CoreEntity? retrievedEntity = await this.TestEntityDatabase.GetEntityAsync(updatedEntity.EntityID, CoreEntityConstants.DefaultSnapshotID);
            _ = retrievedEntity.Should().NotBeNull();
            _ = retrievedEntity!.DisplayName.Should().Be("Updated Entity");
            _ = retrievedEntity.Score.Should().Be(100);
        }

        [Fact]
        public async Task InsertOrReplaceEntityAsync_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            CoreEntity? entity = null;

            // Act & Assert
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.TestEntityDatabase.InsertOrReplaceEntityAsync(entity!));
        }

        [Fact]
        public async Task InsertOrUpdateEntityAsync_NewEntity_ShouldInsert()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            this.OutputEntity(result);

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityIDFromQueryAsync()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            this.OutputEntity(result);

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            Guid? entityID = await this.TestEntityDatabase.GetEntityIDFromQueryAsync("select EntityID from CoreEntity where SnapshotID = ? and json_extract(\"Entity\", '$.DisplayName') = ?", [0, "Updated"]);
            entityID.Should().NotBeNull();
            entityID.Should().Be(result.EntityID);
            entityID.Should().Be(entity.EntityID);
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityIDFromQueryAsync_NoResults()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            this.OutputEntity(result);

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            Guid? entityID = await this.TestEntityDatabase.GetEntityIDFromQueryAsync("select EntityID from CoreEntity where SnapshotID = ? and json_extract(\"Entity\", '$.DisplayName') = ?", [0, "Not Updated"]);
            entityID.Should().BeNull();
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityIDFromQueryAsync_AdditionalSelect()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            this.OutputEntity(result);

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            Guid? entityID = await this.TestEntityDatabase.GetEntityIDFromQueryAsync("select EntityID, CreatedUtc from CoreEntity where SnapshotID = ? and json_extract(\"Entity\", '$.DisplayName') = ?", [0, "Updated"]);
            entityID.Should().NotBeNull();
            entityID.Should().Be(result.EntityID);
            entityID.Should().Be(entity.EntityID);
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityIDFromQueryAsync_InvalidSelect()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            this.OutputEntity(result);

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            _ = await Assert.ThrowsAsync<FormatException>(() => this.TestEntityDatabase.GetEntityIDFromQueryAsync("select CreatedUtc, EntityID from CoreEntity where SnapshotID = ? and json_extract(\"Entity\", '$.DisplayName') = ?", [0, "Updated"]));
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityIDFromPropertyAsync()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            this.OutputEntity(result);

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            Guid? entityID = await this.TestEntityDatabase.GetEntityIDFromPropertyAsync("DisplayName", "Updated", CoreEntityConstants.DefaultSnapshotID);
            entityID.Should().NotBeNull();
            entityID.Should().Be(result.EntityID);
            entityID.Should().Be(entity.EntityID);
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityIDFromPropertyAsync_EntityType()
        {
            // Arrange
            var entity = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            this.OutputEntity(result);

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            Guid? entityID = await this.TestEntityDatabase.GetEntityIDFromPropertyAsync("DisplayName", "Updated", CoreEntityType.HostV1, CoreEntityConstants.DefaultSnapshotID);
            entityID.Should().NotBeNull();
            entityID.Should().Be(result.EntityID);
            entityID.Should().Be(entity.EntityID);

            // Verify that the entity type is respected
            (await this.TestEntityDatabase.GetEntityIDFromPropertyAsync("DisplayName", "Updated", CoreEntityType.ConnectionV1, CoreEntityConstants.DefaultSnapshotID)).Should().BeNull();

            // Verity that the entity type is respected for a flag of the entity type
            (await this.TestEntityDatabase.GetEntityIDFromPropertyAsync("DisplayName", "Updated", CoreEntityType.Version1, CoreEntityConstants.DefaultSnapshotID)).Should().NotBeNull();
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityFromQueryAsync()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            this.OutputEntity(result);

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            CoreEntity? entityResult = await this.TestEntityDatabase.GetEntityFromQueryAsync($"select {CoreEntity.CoreEntitySelectAll} from CoreEntity e where SnapshotID = ? and json_extract(\"Entity\", '$.DisplayName') = ?", [0, "Updated"]);
            entityResult.Should().NotBeNull();

            this.OutputEntity(entityResult);
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityFromPropertyAsync()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            CoreEntity? entityResult = await this.TestEntityDatabase.GetEntityFromPropertyAsync("DisplayName", "Updated", CoreEntityConstants.DefaultSnapshotID);
            entityResult.Should().NotBeNull();

            this.OutputEntity(entityResult);
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityFromPropertyAsync_EntityType()
        {
            // Arrange
            var entity = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            CoreEntity? entityResult = await this.TestEntityDatabase.GetEntityFromPropertyAsync("DisplayName", "Updated", CoreEntityType.HostV1, CoreEntityConstants.DefaultSnapshotID);
            entityResult.Should().NotBeNull();

            this.OutputEntity(entityResult);

            (await this.TestEntityDatabase.GetEntityFromPropertyAsync("DisplayName", "Updated", CoreEntityType.ConnectionV1, CoreEntityConstants.DefaultSnapshotID)).Should().BeNull();
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntitiesFromPropertyAsync()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");
            var entity2 = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            CoreHostEntity? result2 = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity2,
                e => e.LookupKey == entity2.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);

            _ = result2.Should().NotBeNull();
            _ = result2!.EntityID.Should().Be(entity2.EntityID);
            _ = result2.LookupKey.Should().Be(entity2.LookupKey);
            _ = result2.EntityType.Should().Be(entity2.EntityType);
            _ = result2.DisplayName.Should().Be("Updated");
            _ = result2.HostName.Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer);

            List<CoreEntity> entityResults = await this.TestEntityDatabase.GetEntitiesFromPropertyAsync("DisplayName", "Updated", CoreEntityConstants.DefaultSnapshotID);
            entityResults.Should().NotBeNull();
            entityResults.Should().HaveCount(2);

            this.TestOutputHelper.WriteLine($"\n{"All Entities".CenterTitle()}");
            foreach (CoreEntity entityResult in entityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntitiesFromPropertyAsync_EntityType()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");
            var entity2 = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            CoreHostEntity? result2 = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity2,
                e => e.LookupKey == entity2.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);

            _ = result2.Should().NotBeNull();
            _ = result2!.EntityID.Should().Be(entity2.EntityID);
            _ = result2.LookupKey.Should().Be(entity2.LookupKey);
            _ = result2.EntityType.Should().Be(entity2.EntityType);
            _ = result2.DisplayName.Should().Be("Updated");
            _ = result2.HostName.Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer);

            List<CoreEntity> entityResults = await this.TestEntityDatabase.GetEntitiesFromPropertyAsync("DisplayName", "Updated", CoreEntityType.HostV1, CoreEntityConstants.DefaultSnapshotID);
            entityResults.Should().NotBeNull();
            entityResults.Should().HaveCount(1);

            this.TestOutputHelper.WriteLine($"\n{"Host Entities".CenterTitle()}");
            foreach (CoreEntity entityResult in entityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }

            // Verify that a flag of the entity type works
            List<CoreEntity> allEntityResults = await this.TestEntityDatabase.GetEntitiesFromPropertyAsync("DisplayName", "Updated", CoreEntityType.Version1, CoreEntityConstants.DefaultSnapshotID);
            allEntityResults.Should().NotBeNull();
            allEntityResults.Should().HaveCount(2);

            this.TestOutputHelper.WriteLine($"\n{"All Entities".CenterTitle()}");
            foreach (CoreEntity entityResult in allEntityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntitiesFromQueryAsync()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update1");
            var entity2 = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            CoreEntity? result2 = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity2,
                e => e.LookupKey == entity2.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);

            List<CoreEntity> entityResults = await this.TestEntityDatabase.GetEntitiesFromQueryAsync($"select {CoreEntity.CoreEntitySelectAll} from CoreEntity e where SnapshotID = ? and json_extract(\"Entity\", '$.DisplayName') = ?", [0, "Updated"]);
            entityResults.Should().NotBeNull();
            entityResults.Should().HaveCount(2);

            this.TestOutputHelper.WriteLine($"\n{"All Entities".CenterTitle()}");
            foreach (CoreEntity entityResult in entityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntitiesFromPropertyAsync_PartialMatch()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");
            var entity2 = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Not Updated" });

            CoreEntity? result2 = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity2,
                e => e.LookupKey == entity2.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Not Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);

            List<CoreEntity> entityResults = await this.TestEntityDatabase.GetEntitiesFromPropertyAsync("DisplayName", "Updated", CoreEntityConstants.DefaultSnapshotID);
            entityResults.Should().NotBeNull();
            entityResults.Should().HaveCount(1);

            this.TestOutputHelper.WriteLine($"\n{"All Entities".CenterTitle()}");
            foreach (CoreEntity entityResult in entityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntitiesFromQueryAsync_PartialMatch()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update1");
            var entity2 = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Not Updated" });

            CoreHostEntity? result2 = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity2,
                e => e.LookupKey == entity2.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Not Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);

            _ = result2.Should().NotBeNull();
            _ = result2!.EntityID.Should().Be(entity2.EntityID);
            _ = result2.LookupKey.Should().Be(entity2.LookupKey);
            _ = result2.EntityType.Should().Be(entity2.EntityType);
            _ = result2.DisplayName.Should().Be("Updated");
            _ = result2.HostName.Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer);

            List<CoreEntity> entityResults = await this.TestEntityDatabase.GetEntitiesFromQueryAsync($"select {CoreEntity.CoreEntitySelectAll} from CoreEntity e where SnapshotID = ? and json_extract(\"Entity\", '$.DisplayName') = ?", [0, "Updated"]);
            entityResults.Should().NotBeNull();
            entityResults.Should().HaveCount(1);

            this.TestOutputHelper.WriteLine($"\n{"Updated Entities".CenterTitle()}");
            foreach (CoreEntity entityResult in entityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityAsTypeFromPropertyAsync()
        {
            // Arrange
            var entity = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = CoreIPAddressExtensions.StringGooglePublicDnsServer });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer);
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            CoreHostEntity? entityResult = await this.TestEntityDatabase.GetEntityAsTypeFromPropertyAsync<CoreHostEntity>("HostName", CoreIPAddressExtensions.StringGooglePublicDnsServer, CoreEntityConstants.DefaultSnapshotID);
            entityResult.Should().NotBeNull();
            entityResult.HostName.Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer);

            this.OutputEntity(entityResult);
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntityAsTypeFromPropertyAsync_EntityType()
        {
            // Arrange
            var entity = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = CoreIPAddressExtensions.StringGooglePublicDnsServer });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer);
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            CoreHostEntity? entityResult = await this.TestEntityDatabase.GetEntityAsTypeFromPropertyAsync<CoreHostEntity>("HostName", CoreIPAddressExtensions.StringGooglePublicDnsServer, CoreEntityType.HostV1, CoreEntityConstants.DefaultSnapshotID);
            entityResult.Should().NotBeNull();
            entityResult.HostName.Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer);

            this.OutputEntity(entityResult);

            // Verify that the entity type is respected
            (await this.TestEntityDatabase.GetEntityAsTypeFromPropertyAsync<CoreEntity>("HostName", CoreIPAddressExtensions.StringGooglePublicDnsServer, CoreEntityType.ConnectionV1, CoreEntityConstants.DefaultSnapshotID)).Should().BeNull();

            // Verity that the entity type is respected for a flag of the entity type
            (await this.TestEntityDatabase.GetEntityAsTypeFromPropertyAsync<CoreHostEntity>("HostName", CoreIPAddressExtensions.StringGooglePublicDnsServer, CoreEntityType.Version1, CoreEntityConstants.DefaultSnapshotID)).Should().NotBeNull();
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntitiesAsTypeFromPropertyAsync()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");
            var entity2 = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            CoreHostEntity? result2 = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity2,
                e => e.LookupKey == entity2.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);

            _ = result2.Should().NotBeNull();
            _ = result2!.EntityID.Should().Be(entity2.EntityID);
            _ = result2.LookupKey.Should().Be(entity2.LookupKey);
            _ = result2.EntityType.Should().Be(entity2.EntityType);
            _ = result2.DisplayName.Should().Be("Updated");
            _ = result2.HostName.Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer);

            List<CoreHostEntity> entityResults = await this.TestEntityDatabase.GetEntitiesAsTypeFromPropertyAsync<CoreHostEntity>("DisplayName", "Updated", CoreEntityConstants.DefaultSnapshotID);
            entityResults.Should().NotBeNull();
            entityResults.Should().HaveCount(2);

            this.TestOutputHelper.WriteLine($"\n{"Updated Entities".CenterTitle()}");
            foreach (CoreHostEntity entityResult in entityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntitiesAsTypeFromPropertyAsync_EntityType()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");
            var entity2 = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            CoreHostEntity? result2 = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity2,
                e => e.LookupKey == entity2.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);

            _ = result2.Should().NotBeNull();
            _ = result2!.EntityID.Should().Be(entity2.EntityID);
            _ = result2.LookupKey.Should().Be(entity2.LookupKey);
            _ = result2.EntityType.Should().Be(entity2.EntityType);
            _ = result2.DisplayName.Should().Be("Updated");
            _ = result2.HostName.Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer);

            List<CoreHostEntity> entityResults = await this.TestEntityDatabase.GetEntitiesAsTypeFromPropertyAsync<CoreHostEntity>("DisplayName", "Updated", CoreEntityType.HostV1, CoreEntityConstants.DefaultSnapshotID);
            entityResults.Should().NotBeNull();
            entityResults.Should().HaveCount(1);

            this.TestOutputHelper.WriteLine($"\n{"Host Entities".CenterTitle()}");
            foreach (CoreHostEntity entityResult in entityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }

            // Verify that a flag of the entity type works
            List<CoreHostEntity> allEntityResults = await this.TestEntityDatabase.GetEntitiesAsTypeFromPropertyAsync<CoreHostEntity>("DisplayName", "Updated", CoreEntityType.Version1, CoreEntityConstants.DefaultSnapshotID);
            allEntityResults.Should().NotBeNull();
            allEntityResults.Should().HaveCount(2);

            this.TestOutputHelper.WriteLine($"\n{"All Entities".CenterTitle()}");
            foreach (CoreHostEntity entityResult in allEntityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntitiesAsTypeFromQueryAsync()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update1");
            var entity2 = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            CoreEntity? result2 = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity2,
                e => e.LookupKey == entity2.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);

            List<CoreHostEntity> entityResults = await this.TestEntityDatabase.GetEntitiesAsTypeFromQueryAsync<CoreHostEntity>($"select {CoreEntity.CoreEntitySelectAll} from CoreEntity e where SnapshotID = ? and json_extract(\"Entity\", '$.DisplayName') = ?", [0, "Updated"]);
            entityResults.Should().NotBeNull();
            entityResults.Should().HaveCount(2);

            this.TestOutputHelper.WriteLine($"\n{"Updated Entities".CenterTitle()}");
            foreach (CoreHostEntity entityResult in entityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntitiesAsTypeFromPropertyAsync_PartialMatch()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");
            var entity2 = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Not Updated" });

            CoreEntity? result2 = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity2,
                e => e.LookupKey == entity2.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Not Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);

            List<CoreHostEntity> entityResults = await this.TestEntityDatabase.GetEntitiesAsTypeFromPropertyAsync<CoreHostEntity>("DisplayName", "Updated", CoreEntityConstants.DefaultSnapshotID);
            entityResults.Should().NotBeNull();
            entityResults.Should().HaveCount(1);

            this.TestOutputHelper.WriteLine($"\n{"Updated Entities".CenterTitle()}");
            foreach (CoreHostEntity entityResult in entityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task EntityDatabaseIntegration_GetEntitiesAsTypeFromQueryAsync_PartialMatch()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update1");
            var entity2 = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                e => e.LookupKey == entity.LookupKey,
                existing => existing with { DisplayName = "Not Updated" });

            CoreEntity? result2 = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity2,
                e => e.LookupKey == entity2.LookupKey,
                existing => existing with { DisplayName = "Updated" });

            // Assert
            _ = result.Should().NotBeNull();

            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
            _ = result.DisplayName.Should().Be("Not Updated");
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);

            List<CoreHostEntity> entityResults = await this.TestEntityDatabase.GetEntitiesAsTypeFromQueryAsync<CoreHostEntity>($"select {CoreEntity.CoreEntitySelectAll} from CoreEntity e where SnapshotID = ? and json_extract(\"Entity\", '$.DisplayName') = ?", [0, "Updated"]);
            entityResults.Should().NotBeNull();
            entityResults.Should().HaveCount(1);

            this.TestOutputHelper.WriteLine($"\n{"Updated Entities".CenterTitle()}");
            foreach (CoreHostEntity entityResult in entityResults)
            {
                this.OutputEntity(entityResult);
                this.TestOutputHelper.WriteLine();
            }
        }

        [Fact]
        public async Task InsertOrUpdateEntityAsync_ExistingEntity_ShouldUpdate()
        {
            // Arrange
            var originalEntity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-update");
            _ = await this.TestEntityDatabase.InsertEntityAsync(originalEntity);

            var updateEntity = new CoreEntity(originalEntity) { DisplayName = "New Name" };

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                updateEntity,
                e => e.EntityID == originalEntity.EntityID,
                existing => existing with { DisplayName = updateEntity.DisplayName, Score = 200 });

            // Assert
            _ = result.Should().NotBeNull();

            this.OutputEntity(result);

            _ = result!.DisplayName.Should().Be("New Name");
            _ = result.Score.Should().Be(200);
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task InsertOrUpdateEntityAsync_NullParameters_ShouldThrowArgumentNullException()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity");

            // Act & Assert
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.TestEntityDatabase.InsertOrUpdateEntityAsync<CoreEntity>(null!, e => true, e => e));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.TestEntityDatabase.InsertOrUpdateEntityAsync(entity, (Type)null!, e => e));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.TestEntityDatabase.InsertOrUpdateEntityAsync(entity, e => true, null!));
        }

        [Fact]
        public async Task InsertOrUpdateEntityAsync_CoreEntityOverload_ShouldWork()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-core");

            // Act
            CoreEntity? result = await this.TestEntityDatabase.InsertOrUpdateEntityAsync(
                entity,
                typeof(CoreEntity),
                existing => existing with { DisplayName = "Updated via Core" });

            // Assert
            _ = result.Should().NotBeNull();
            _ = result!.EntityID.Should().Be(entity.EntityID);
        }

        [Fact]
        public async Task GetEntityAsTypeAsync_ByEntityID_ShouldReturnEntity()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-get");
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreEntity>(
                entity.EntityID, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
            _ = result.EntityType.Should().Be(entity.EntityType);
        }

        [Fact]
        public async Task GetEntityAsTypeAsync_ByLookupKey_ShouldReturnEntity()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-lookup");
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreEntity>(
                entity.LookupKey, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result!.EntityID.Should().Be(entity.EntityID);
            _ = result.LookupKey.Should().Be(entity.LookupKey);
        }

        [Fact]
        public async Task GetEntityAsTypeAsync_ByLookupKey_EntityType_ShouldReturnEntity()
        {
            // Arrange
            var hostEntity = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);
            _ = await this.TestEntityDatabase.InsertEntityAsync(hostEntity);

            // Act
            CoreHostEntity? result = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreHostEntity>(
                hostEntity.LookupKey, CoreEntityType.HostV1, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result!.EntityID.Should().Be(hostEntity.EntityID);
            _ = result.LookupKey.Should().Be(hostEntity.LookupKey);
            _ = result.EntityType.Should().Be(hostEntity.EntityType);
            _ = result.HostName.Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer);

            (await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreHostEntity>(
                hostEntity.LookupKey, CoreEntityType.ConnectionV1, CoreEntityConstants.DefaultSnapshotID)).Should().BeNull();
        }

        [Fact]
        public async Task GetEntityAsync_ByEntityID_ShouldReturnEntity()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-get-base");
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.GetEntityAsync(
                entity.EntityID, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result!.EntityID.Should().Be(entity.EntityID);
        }

        [Fact]
        public async Task GetEntityAsync_ByLookupKey_ShouldReturnEntity()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-lookup-base");
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity);

            // Act
            CoreEntity? result = await this.TestEntityDatabase.GetEntityAsync(
                entity.LookupKey, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result!.EntityID.Should().Be(entity.EntityID);
        }

        [Fact]
        public async Task GetEntityIDAsync_ValidLookupKey_ShouldReturnEntityID()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-id");
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity);

            // Act
            Guid? result = await this.TestEntityDatabase.GetEntityIDAsync(
                entity.LookupKey, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Should().Be(entity.EntityID);
        }

        [Fact]
        public async Task GetEntityIDAsync_InvalidLookupKey_ShouldReturnNull()
        {
            // Arrange
            const string invalidLookupKey = "non-existent-key";

            // Act
            Guid? result = await this.TestEntityDatabase.GetEntityIDAsync(
                invalidLookupKey, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().BeNull();
        }

        [Fact]
        public async Task GetEntitiesAsync_ShouldReturnAllEntities()
        {
            // Arrange
            var entities = new List<CoreEntity>
            {
                new(CoreEntityType.EntityV1, "entity-1"),
                new(CoreEntityType.EntityV1, "entity-2"),
                new(CoreEntityType.EntityV1, "entity-3"),
            };

            foreach (CoreEntity entity in entities)
            {
                _ = await this.TestEntityDatabase.InsertEntityAsync(entity);
            }

            // Act
            List<CoreEntity> result = await this.TestEntityDatabase.GetEntitiesAsync(CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Should().HaveCount(3);
            _ = result.Select(e => e.EntityID).Should().BeEquivalentTo(entities.Select(e => e.EntityID));
        }

        [Fact]
        public async Task GetEntitiesOfTypeAsync_ShouldReturnAllEntitiesOfType()
        {
            // Arrange
            var entities = new List<CoreEntity>
            {
                new(CoreEntityType.EntityV1, "entity-1"),
                new(CoreEntityType.HostV1, "entity-2"),
                new(CoreEntityType.NetworkAddressV1, "entity-3"),
            };

            foreach (CoreEntity entity in entities)
            {
                _ = await this.TestEntityDatabase.InsertEntityAsync(entity);
            }

            // Act
            List<CoreEntity> result = await this.TestEntityDatabase.GetEntitiesAsync(CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Should().HaveCount(3);
            _ = result.Select(e => e.EntityID).Should().BeEquivalentTo(entities.Select(e => e.EntityID));

            // Assert for GetEntitiesAsync
            List<CoreEntity> result2 = await this.TestEntityDatabase.GetEntitiesOfTypeAsync(CoreEntityType.HostV1, CoreEntityConstants.DefaultSnapshotID);
            _ = result2.Should().NotBeNull();
            _ = result2.Should().HaveCount(1);

            CoreEntity hostEntity = result2.First();

            this.OutputEntity(hostEntity);
            hostEntity.EntityID.Should().Be(entities[1].EntityID);
        }

        [Fact]
        public async Task GetEntitiesAsTypeAsync_AllEntities_ShouldReturnAllEntities()
        {
            // Arrange
            var entities = new List<CoreEntity>
            {
                new(CoreEntityType.EntityV1, "entity-1"),
                new(CoreEntityType.EntityV1, "entity-2"),
            };

            foreach (CoreEntity entity in entities)
            {
                _ = await this.TestEntityDatabase.InsertEntityAsync(entity);
            }

            // Act
            List<CoreEntity> result = await this.TestEntityDatabase.GetEntitiesAsTypeAsync<CoreEntity>(
                CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetEntitiesAsTypeAsync_WithEntityIDs_ShouldReturnFilteredEntities()
        {
            // Arrange
            var entities = new List<CoreEntity>
            {
                new(CoreEntityType.EntityV1, "entity-1"),
                new(CoreEntityType.EntityV1, "entity-2"),
                new(CoreEntityType.EntityV1, "entity-3"),
            };

            foreach (CoreEntity entity in entities)
            {
                _ = await this.TestEntityDatabase.InsertEntityAsync(entity);
            }

            var entityIDs = new List<Guid> { entities[0].EntityID, entities[2].EntityID };

            // Act
            List<CoreEntity> result = await this.TestEntityDatabase.GetEntitiesAsTypeAsync<CoreEntity>(
                entityIDs, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Should().HaveCount(2);
            _ = result.Select(e => e.EntityID).Should().BeEquivalentTo(entityIDs);
        }

        [Fact]
        public async Task GetEntitiesAsTypeAsync_WithPredicate_ShouldReturnFilteredEntities()
        {
            // Arrange
            var entities = new List<CoreEntity>
            {
                new(CoreEntityType.EntityV1, "entity-1") { Score = 100 },
                new(CoreEntityType.EntityV1, "entity-2") { Score = 200 },
                new(CoreEntityType.EntityV1, "entity-3") { Score = 100 },
            };

            foreach (CoreEntity entity in entities)
            {
                _ = await this.TestEntityDatabase.InsertEntityAsync(entity);
            }

            // Act
            List<CoreEntity> result = await this.TestEntityDatabase.GetEntitiesAsTypeAsync<CoreEntity>(
                e => e.Score == 100, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Should().HaveCount(2);
            _ = result.Should().OnlyContain(e => e.Score == 100);
        }

        [Fact]
        public async Task GetEntitiesAsTypeAsync_WithCoreEntityPredicate_ShouldReturnFilteredEntities()
        {
            // Arrange
            var entities = new List<CoreEntity>
            {
                new(CoreEntityType.EntityV1, "entity-1") { Score = 100 },
                new(CoreEntityType.EntityV1, "entity-2") { Score = 200 },
            };

            foreach (CoreEntity entity in entities)
            {
                _ = await this.TestEntityDatabase.InsertEntityAsync(entity);
            }

            // Act
            List<CoreEntity> result = await this.TestEntityDatabase.GetEntitiesAsTypeAsync<CoreEntity>(
                (CoreEntity e) => e.Score == 200, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Should().HaveCount(1);
            _ = result.First().Score.Should().Be(200);
        }

        [Fact]
        public async Task GetEntitiesAsTypeAsync_WithCoreEntityPredicateNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            Func<CoreEntity, bool>? predicate = null;

            // Act & Assert
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.TestEntityDatabase.GetEntitiesAsTypeAsync<CoreEntity>(predicate!, CoreEntityConstants.DefaultSnapshotID));
        }

        [Fact]
        public async Task DeleteEntityAsync_ValidEntity_ShouldDeleteEntity()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-delete");
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity);

            // Verify entity exists
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(1);

            // Act
            int result = await this.TestEntityDatabase.DeleteEntityAsync(entity, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().BeGreaterThan(0);
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DeleteEntityAsync_NonExistentEntity_ShouldReturnZero()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "non-existent-entity");

            // Act
            int result = await this.TestEntityDatabase.DeleteEntityAsync(entity, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().Be(0);
        }

        [Fact]
        public async Task DeleteEntityAsync_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            CoreEntity? entity = null;

            // Act & Assert
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.TestEntityDatabase.DeleteEntityAsync(entity!, CoreEntityConstants.DefaultSnapshotID));
        }

        [Fact]
        public void ConvertEntity_ValidEntity_ShouldConvertSuccessfully()
        {
            // Arrange
            var originalEntity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-convert");
            var coreEntity = new CoreEntity
            {
                EntityID = originalEntity.EntityID,
                LookupKey = originalEntity.LookupKey,
                EntityType = originalEntity.EntityType,
                Entity = originalEntity.Entity,
            };

            // Act
            CoreEntity? result = this.TestEntityDatabase.ConvertEntity<CoreEntity>(coreEntity);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result!.EntityID.Should().Be(originalEntity.EntityID);
            _ = result.LookupKey.Should().Be(originalEntity.LookupKey);
            _ = result.EntityType.Should().Be(originalEntity.EntityType);
        }

        [Fact]
        public void ConvertEntity_ValidEntity_ShouldConvertToEntitySuccessfully()
        {
            // Arrange
            var hostEntity = new CoreHostEntity(CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer);

            // Act
            CoreEntity? result = this.TestEntityDatabase.ConvertEntity<CoreEntity>(hostEntity);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result!.EntityID.Should().Be(hostEntity.EntityID);
            _ = result.LookupKey.Should().Be(hostEntity.LookupKey);
            _ = result.EntityType.Should().Be(hostEntity.EntityType);

            this.OutputEntity(result);
        }

        [Fact]
        public void ConvertEntity_NullEntity_ShouldReturnNull()
        {
            // Arrange
            CoreEntity? entity = null;

            // Act
            CoreEntity? result = this.TestEntityDatabase.ConvertEntity<CoreEntity>(entity);

            // Assert
            _ = result.Should().BeNull();
        }

        [Fact]
        public void ConvertEntity_WithCustomJsonOptions_ShouldUseOptions()
        {
            // Arrange
            var originalEntity = new CoreEntity(CoreEntityType.EntityV1, "test-entity-convert-options");
            var coreEntity = new CoreEntity
            {
                EntityID = originalEntity.EntityID,
                LookupKey = originalEntity.LookupKey,
                EntityType = originalEntity.EntityType,
                Entity = originalEntity.Entity,
            };

            JsonSerializerOptions customOptions = CoreDefaultJsonSerializerOptions.DefaultFormatted;

            // Act
            CoreEntity? result = this.TestEntityDatabase.ConvertEntity<CoreEntity>(coreEntity, customOptions);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result!.EntityID.Should().Be(originalEntity.EntityID);
        }

        [Fact]
        public async Task ConnectionEntityTable_InsertAndRetrieve_ShouldWork()
        {
            // Arrange
            var connectionEntity = new CoreConnectionEntity
            {
                EntityID = Guid.NewGuid(),
                LookupKey = "test-connection",
                EntityType = CoreEntityType.ConnectionV1,
                ConnectionEntityType = CoreConnectionEntityType.GenericConnection,
                FromEntityID = Guid.NewGuid(),
                ToEntityID = Guid.NewGuid(),
            };

            // Act
            int insertResult = await this.TestEntityDatabase.SQLiteAsyncConnection.InsertAsync(connectionEntity);
            CoreConnectionEntity? retrievedEntity = await this.TestEntityDatabase.ConnectionEntityTable.FirstOrDefaultAsync();

            // Assert
            _ = insertResult.Should().BeGreaterThan(0);
            _ = retrievedEntity.Should().NotBeNull();
            _ = retrievedEntity!.EntityID.Should().Be(connectionEntity.EntityID);
            _ = retrievedEntity.FromEntityID.Should().Be(connectionEntity.FromEntityID);
            _ = retrievedEntity.ToEntityID.Should().Be(connectionEntity.ToEntityID);
        }

        [Fact]
        public async Task SnapshotID_DifferentSnapshots_ShouldIsolateEntities()
        {
            // Arrange
            const int snapshot1 = 1;
            const int snapshot2 = 2;

            var entity1 = new CoreEntity(CoreEntityType.EntityV1, "entity-snapshot-1") { SnapshotID = snapshot1 };
            var entity2 = new CoreEntity(CoreEntityType.EntityV1, "entity-snapshot-2") { SnapshotID = snapshot2 };

            // Act
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity1);
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity2);

            // Assert
            List<CoreEntity> entitiesSnapshot1 = await this.TestEntityDatabase.GetEntitiesAsync(snapshot1);
            List<CoreEntity> entitiesSnapshot2 = await this.TestEntityDatabase.GetEntitiesAsync(snapshot2);

            _ = entitiesSnapshot1.Should().HaveCount(1);
            _ = entitiesSnapshot2.Should().HaveCount(1);
            _ = entitiesSnapshot1.First().EntityID.Should().Be(entity1.EntityID);
            _ = entitiesSnapshot2.First().EntityID.Should().Be(entity2.EntityID);
        }

        [Fact]
        public async Task SnapshotID_DefaultSnapshot_ShouldWork()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "entity-default-snapshot");

            // Act
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity);
            CoreEntity? result = await this.TestEntityDatabase.GetEntityAsync(
                entity.EntityID, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result!.EntityID.Should().Be(entity.EntityID);
        }

        [Fact]
        public async Task GetEntityAsTypeAsync_NonExistentEntity_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            CoreEntity? result = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreEntity>(
                nonExistentId, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().BeNull();
        }

        [Fact]
        public async Task GetEntityAsTypeAsync_NonExistentLookupKey_ShouldReturnNull()
        {
            // Arrange
            const string nonExistentKey = "non-existent-lookup-key";

            // Act
            CoreEntity? result = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreEntity>(
                nonExistentKey, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().BeNull();
        }

        [Fact(Skip = "Performance")]
        public async Task BulkOperations_InsertManyEntities_ShouldPerformWell()
        {
            // Arrange
            const int entityCount = 1000;
            var entities = new List<CoreEntity>();

            for (int i = 0; i < entityCount; i++)
            {
                entities.Add(new CoreEntity(CoreEntityType.EntityV1, $"bulk-entity-{i}"));
            }

            // Act
            DateTime startTime = DateTime.UtcNow;

            foreach (CoreEntity entity in entities)
            {
                _ = await this.TestEntityDatabase.InsertEntityAsync(entity);
            }

            DateTime endTime = DateTime.UtcNow;
            TimeSpan duration = endTime - startTime;

            // Assert
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(entityCount);

            // Performance assertion - should complete within reasonable time
            _ = duration.TotalSeconds.Should().BeLessThan(100); // Adjust based on expected performance

            this.TestOutputHelper.WriteLine($"Inserted {entityCount} entities in {duration.TotalMilliseconds:F2} ms");
        }

        [Fact]
        public async Task BulkOperations_RetrieveManyEntities_ShouldPerformWell()
        {
            // Arrange
            const int entityCount = 100;
            var entities = new List<CoreEntity>();

            for (int i = 0; i < entityCount; i++)
            {
                var entity = new CoreEntity(CoreEntityType.EntityV1, $"retrieve-entity-{i}");
                entities.Add(entity);
                _ = await this.TestEntityDatabase.InsertEntityAsync(entity);
            }

            // Act
            DateTime startTime = DateTime.UtcNow;
            List<CoreEntity> retrievedEntities = await this.TestEntityDatabase.GetEntitiesAsync(CoreEntityConstants.DefaultSnapshotID);
            DateTime endTime = DateTime.UtcNow;
            TimeSpan duration = endTime - startTime;

            // Assert
            _ = retrievedEntities.Should().HaveCount(entityCount);
            _ = duration.TotalSeconds.Should().BeLessThan(5); // Should be fast for retrieval

            this.TestOutputHelper.WriteLine($"Retrieved {entityCount} entities in {duration.TotalMilliseconds:F2} ms");
        }

        [Fact]
        public async Task ComplexScenario_FullWorkflow_ShouldWorkCorrectly()
        {
            // Arrange
            var originalEntity = new CoreEntity(CoreEntityType.EntityV1, "complex-workflow-entity")
            {
                DisplayName = "Original Entity",
                Score = 100,
            };

            // Act & Assert - Insert
            int insertResult = await this.TestEntityDatabase.InsertEntityAsync(originalEntity);
            _ = insertResult.Should().BeGreaterThan(0);

            // Act & Assert - Retrieve
            CoreEntity? retrievedEntity = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreEntity>(
                originalEntity.EntityID, CoreEntityConstants.DefaultSnapshotID);
            _ = retrievedEntity.Should().NotBeNull();
            _ = retrievedEntity!.DisplayName.Should().Be("Original Entity");

            // Act & Assert - Update
            var updatedEntity = new CoreEntity(retrievedEntity)
            {
                DisplayName = "Updated Entity",
                Score = 200,
            };

            int updateResult = await this.TestEntityDatabase.InsertOrReplaceEntityAsync(updatedEntity);
            _ = updateResult.Should().BeGreaterThan(0);

            // Act & Assert - Verify Update
            CoreEntity? finalEntity = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreEntity>(
                originalEntity.EntityID, CoreEntityConstants.DefaultSnapshotID);
            _ = finalEntity.Should().NotBeNull();
            _ = finalEntity!.DisplayName.Should().Be("Updated Entity");
            _ = finalEntity.Score.Should().Be(200);

            // Act & Assert - Delete
            int deleteResult = await this.TestEntityDatabase.DeleteEntityAsync(finalEntity, CoreEntityConstants.DefaultSnapshotID);
            _ = deleteResult.Should().BeGreaterThan(0);

            // Act & Assert - Verify Deletion
            CoreEntity? deletedEntity = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreEntity>(
                originalEntity.EntityID, CoreEntityConstants.DefaultSnapshotID);
            _ = deletedEntity.Should().BeNull();
        }

        [Fact]
        public async Task ComplexScenario_MultipleEntityTypes_ShouldWorkCorrectly()
        {
            // Arrange
            var entity1 = new CoreEntity(CoreEntityType.EntityV1, "multi-type-entity-1");
            var entity2 = new CoreEntity(CoreEntityType.EntityV1, "multi-type-entity-2");
            var connectionEntity = new CoreConnectionEntity
            {
                EntityID = Guid.NewGuid(),
                LookupKey = "connection-between-entities",
                EntityType = CoreEntityType.ConnectionV1,
                ConnectionEntityType = CoreConnectionEntityType.GenericConnection,
                FromEntityID = entity1.EntityID,
                ToEntityID = entity2.EntityID,
            };

            // Act
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity1);
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity2);
            _ = await this.TestEntityDatabase.SQLiteAsyncConnection.InsertAsync(connectionEntity);

            // Assert
            _ = (await this.TestEntityDatabase.EntityTable.CountAsync()).Should().Be(2);
            _ = (await this.TestEntityDatabase.ConnectionEntityTable.CountAsync()).Should().Be(1);

            CoreConnectionEntity? retrievedConnection = await this.TestEntityDatabase.ConnectionEntityTable.FirstOrDefaultAsync();
            _ = retrievedConnection.Should().NotBeNull();
            _ = retrievedConnection!.FromEntityID.Should().Be(entity1.EntityID);
            _ = retrievedConnection.ToEntityID.Should().Be(entity2.EntityID);
        }

        [Fact]
        public async Task Validation_EntityProperties_ShouldBeCorrect()
        {
            // Arrange
            var entity = new CoreEntity(CoreEntityType.EntityV1, "validation-entity")
            {
                DisplayName = "Test Entity",
                Score = 500,
                Source = "Test Source",
            };

            // Act
            _ = await this.TestEntityDatabase.InsertEntityAsync(entity);
            CoreEntity? retrievedEntity = await this.TestEntityDatabase.GetEntityAsTypeAsync<CoreEntity>(
                entity.EntityID, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = retrievedEntity.Should().NotBeNull();

            // Validate all properties
            this.ValidateAndOutputEntity(retrievedEntity!, CoreEntityType.EntityV1, 500, "validation-entity");

            _ = retrievedEntity!.DisplayName.Should().Be("Test Entity");
            _ = retrievedEntity.Source.Should().Be("Test Source");
            _ = retrievedEntity.LookupKey.Should().Be(entity.LookupKey);
            _ = retrievedEntity.EntityOwnerID.Should().Be(CoreEntityConstants.NoneOwnerEntityID);
            _ = retrievedEntity.CreatedUtc.Should().Be(retrievedEntity.ModifiedUtc);
        }
    }
}
