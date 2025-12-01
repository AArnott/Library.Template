// Assembly         : NetworkVisor.Platform.Test.Shared
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// // ***********************************************************************
// <copyright file="CoreEntityTestCaseBase.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Database;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Metadata.Database;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Fixtures;
using NetworkVisor.Platform.Test.Fixtures;
using Xunit;

namespace NetworkVisor.Platform.Test.TestCase
{
    /// <summary>
    /// Represents the base class for core test cases in the NetworkVisor platform.
    /// </summary>
    /// <remarks>
    /// This abstract class provides a foundational implementation for test cases, integrating
    /// with xUnit and offering various utilities and services for testing within the NetworkVisor platform.
    /// </remarks>
    public abstract class CoreEntityTestCaseBase : CoreTestClassBase, IClassFixture<CoreTestClassFixture>
    {
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEntityTestCaseBase"/> class.
        /// </summary>
        /// <param name="testClassFixture">The test class fixture.</param>
        /// <remarks>
        ///     TestClassFixture is shared across test cases within the same test class.
        /// </remarks>
        protected CoreEntityTestCaseBase(ICoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.TemporaryEntityDatabasePath = CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath(this.TestFileSystem, CoreAppConstants.EntityDatabaseName);
            this.TemporaryMetadataEntityDatabasePath = CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath(this.TestFileSystem, CoreAppConstants.MetadataEntityDatabaseName);
            this.TestEntityDatabase = new CoreEntityDatabase(this.TestFileSystem, this.TemporaryEntityDatabasePath);
            this.TestMetadataEntityDatabase = new CoreMetadataEntityDatabase(this.TestFileSystem, this.TemporaryMetadataEntityDatabasePath);
        }

        protected string TemporaryEntityDatabasePath { get; }

        protected string TemporaryMetadataEntityDatabasePath { get; }

        protected ICoreEntityDatabase TestEntityDatabase { get; }

        protected ICoreMetadataEntityDatabase TestMetadataEntityDatabase { get; }

        /// <summary>
        /// Validates the specified <paramref name="entity"/> against the provided parameters and outputs its details.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of the entity, which must derive from <see cref="CoreEntity"/>.
        /// </typeparam>
        /// <param name="entity">
        /// The entity to validate. Must not be <c><see langword="null"/></c>.
        /// </param>
        /// <param name="entityType">
        /// The expected type of the entity.
        /// </param>
        /// <param name="score">
        /// The expected score of the entity.
        /// </param>
        /// <param name="lookupKey">
        /// An optional lookup key for the entity. If not provided, a default lookup key is generated.
        /// </param>
        /// <remarks>
        /// This method performs a series of validations to ensure the integrity and correctness of the entity.
        /// It also outputs the entity's details in JSON format for debugging purposes.
        /// </remarks>
        /// <exception cref="FluentAssertions.Execution.AssertionFailedException">
        /// Thrown if any of the validation checks fail.
        /// </exception>
        protected void ValidateAndOutputEntity<TEntity>(TEntity entity, CoreEntityType entityType, ulong score, string? lookupKey = null)
            where TEntity : CoreEntity
        {
            DateTimeOffset currentUtc = DateTimeOffset.UtcNow;
            entity.Should().NotBeNull().And.Subject.Should().BeAssignableTo<TEntity>();
            var jsonString = JsonSerializer.Serialize(entity, typeof(TEntity), CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider));
            this.TestOutputHelper.WriteLine($"{$"Entity: {entity.EntityID}".CenterTitle()}\n{jsonString}");

            string formattedJson = CoreSerializableObject.FormatJson(entity.Entity);
            this.TestOutputHelper.WriteLine($"\n{$"Converted Entity: {entity.EntityID}".CenterTitle()}\n{formattedJson}");

            lookupKey ??= CoreEntityConstants.CreateLookupKeyFromEntityID(entity.EntityID);

            entity.LookupKey.Should().Be(lookupKey);
            entity.Entity.Should().Be(CoreEntityBase.CalculateEntityJson(entity));
            entity.EntityOwnerID.Should().Be(CoreEntityConstants.NoneOwnerEntityID);
            entity.CreatedUtc.Should().Be(entity.ModifiedUtc);

            // Check if CreatedUtc is set to a valid value
            if (entity.CreatedUtc != DateTime.MinValue)
            {
                entity.CreatedUtc.Should().BeCloseTo(currentUtc, TimeSpan.FromSeconds(15));
            }

            entity.EntityType.Should().Be(entityType);
            entity.Score.Should().Be(score);
            entity.TimeToLive.Should().Be(CoreEntityConstants.DefaultTimeToLiveInSecs);
        }

        protected void OutputEntity(CoreEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var jsonString = JsonSerializer.Serialize(entity, CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider));
            this.TestOutputHelper.WriteLine($"{$"Entity: {entity.EntityID}".CenterTitle()}\n{jsonString}");

            this.TestOutputHelper.WriteLine($"\n{$"Converted Entity: {entity.EntityID}".CenterTitle()}\n{CoreSerializableObject.FormatJson(entity.Entity)}");
        }

        protected override void Dispose(bool disposing)
        {
            if (this._disposedValue)
            {
                return;
            }

            try
            {
                this.TestEntityDatabase?.Dispose();
                this.TestMetadataEntityDatabase?.Dispose();
                this.TestFileSystem.WaitToDeleteLockedFile(this.TemporaryEntityDatabasePath);
                this.TestFileSystem.WaitToDeleteLockedFile(this.TemporaryMetadataEntityDatabasePath);
            }
            finally
            {
                this._disposedValue = true;
                base.Dispose(disposing);
            }
        }
    }
}
