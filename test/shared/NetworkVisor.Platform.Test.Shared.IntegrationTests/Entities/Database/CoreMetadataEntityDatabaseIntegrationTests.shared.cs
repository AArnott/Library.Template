// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreMetadataEntityDatabaseIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Database;
using NetworkVisor.Core.Entities.Metadata;
using NetworkVisor.Core.Entities.Metadata.Base;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Metadata.Database;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Entities.Database
{
    /// <summary>
    /// Class CoreMetadataEntityDatabaseIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreMetadataEntityDatabaseIntegrationTests))]
    public class CoreMetadataEntityDatabaseIntegrationTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreMetadataEntityDatabaseIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreMetadataEntityDatabaseIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void MetadataEntityDatabaseIntegration_EntityDatabase()
        {
            _ = this.TestMetadataEntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMetadataEntityDatabase>();

            // Ensure the database is initialized and the CoreSQLiteAsyncConnection is set up correctly
            _ = this.TestMetadataEntityDatabase.SQLiteAsyncConnection.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreSQLiteAsyncConnection>();
            _ = this.TestFileSystem.FileExists(this.TemporaryMetadataEntityDatabasePath).Should().BeTrue();
        }

        [Fact]
        public async Task MetadataEntityDatabaseIntegration_Table_Initial()
        {
            _ = this.TestMetadataEntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMetadataEntityDatabase>();
            (await this.TestMetadataEntityDatabase.SQLiteAsyncConnection.Table<CoreMetadataEntity>().CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task MetadataEntityDatabaseIntegration_Empty_DeleteAllAsync()
        {
            _ = this.TestMetadataEntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMetadataEntityDatabase>();
            _ = (await this.TestMetadataEntityDatabase.SQLiteAsyncConnection.DeleteAllAsync<CoreMetadataEntity>()).Should().Be(0);
        }

        [Fact]
        public void MetadataEntityDatabaseIntegration_Entity()
        {
            _ = this.TestMetadataEntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMetadataEntityDatabase>();

            var entity = new CoreMetadataEntity();
            entity.LookupKey.Should().Be(CoreMetadataEntityType.Unknown.GetDescription());
            entity.Entity.Should().Be(CoreEntityBase.CalculateEntityJson(entity));

            this.OutputEntity(entity!);
        }

        [Fact]
        public async Task MetadataEntityDatabaseIntegration_InsertAsync()
        {
            _ = this.TestMetadataEntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMetadataEntityDatabase>();

            var entity = new CoreMetadataEntity();
            entity.LookupKey.Should().Be(CoreMetadataEntityType.Unknown.GetDescription());
            entity.Entity.Should().Be(CoreEntityBase.CalculateEntityJson(entity));

            _ = (await this.TestMetadataEntityDatabase.SQLiteAsyncConnection.InsertAsync(entity)).Should().NotBe(0);
            (await this.TestMetadataEntityDatabase.SQLiteAsyncConnection.Table<CoreMetadataEntity>().CountAsync()).Should().Be(1);

            CoreMetadataEntity? insertedEntity = await this.TestMetadataEntityDatabase.SQLiteAsyncConnection.Table<CoreMetadataEntity>().FirstOrDefaultAsync();
            insertedEntity.Should().NotBeNull().And.Subject.Should().BeOfType<CoreMetadataEntity>();

            this.OutputEntity(insertedEntity!);

            insertedEntity.Should().BeEquivalentTo(entity);
        }
    }
}
