// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreEntityUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using FluentAssertions;
using Moq;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Database;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.UnitTests.Entities
{
    /// <summary>
    /// Class CoreEntityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreEntityUnitTests))]
    public class CoreEntityUnitTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void EntityUnit_Entity_Default()
        {
            this.ValidateAndOutputEntity<CoreEntity>(new CoreEntity(), CoreEntityType.EntityV1, CoreEntityConstants.DefaultEntityScore);
        }

        [Fact]
        public void CreateHashedEntityID_ShouldReturnEmptyGuid_WhenInputIsNull()
        {
            // Arrange
            string? input = null;

            // Act
            Guid result = CoreEntityConstants.CreateHashedEntityID(input);

            // Assert
            Assert.Equal(CoreEntityConstants.NoneEntityID, result);
        }

        [Fact]
        public void CreateHashedEntityID_ShouldReturnEmptyGuid_WhenInputIsEmpty()
        {
            // Arrange
            string input = string.Empty;

            // Act
            Guid result = CoreEntityConstants.CreateHashedEntityID(input);

            // Assert
            Assert.Equal(CoreEntityConstants.NoneEntityID, result);
        }

        [Fact]
        public void CreateHashedEntityID_ShouldReturnEmptyGuid_WhenInputIsWhitespace()
        {
            // Arrange
            string input = "   ";

            // Act
            Guid result = CoreEntityConstants.CreateHashedEntityID(input);

            // Assert
            Assert.Equal(CoreEntityConstants.NoneEntityID, result);
        }

        [Fact]
        public void CreateHashedEntityID_ShouldReturnHashedGuid_WhenInputIsValid()
        {
            // Arrange
            string input = "validInput";

            // Act
            Guid result = CoreEntityConstants.CreateHashedEntityID(input);

            // Assert
            Assert.NotEqual(CoreEntityConstants.NoneEntityID, result);
        }

        [Fact]
        public void CreateHashedEntityID_ShouldReturnNewGuid_WhenHashingFails()
        {
            // Arrange
            string input = "invalidInput";

            // Mock or simulate `ToHashedGuid` returning null if necessary
            // Act
            Guid result = CoreEntityConstants.CreateHashedEntityID(input);

            // Assert
            Assert.NotEqual(CoreEntityConstants.NoneEntityID, result);
        }

        [Theory]
        [InlineData("testInput1", "testInput1")]
        [InlineData("testInput2", "TESTInput2")]
        public void CreateHashedEntityID_ShouldReturnSameGuid_WhenInputIsSame(string input1, string input2)
        {
            // Arrange
            Guid expected = CoreEntityConstants.CreateHashedEntityID(input1);

            // Act
            Guid result = CoreEntityConstants.CreateHashedEntityID(input2);

            // Assert
            Assert.Equal(expected, result);
        }

        // Mocking is not supported on iOS and MacCatalyst platforms
#if !NV_PLAT_IOS && !NV_PLAT_MACCATALYST

        [Fact]
        public async Task InsertEntityAsync_ShouldInsertEntitySuccessfully()
        {
            // Arrange
            var mockDatabase = new Mock<ICoreEntityDatabase>();
            var testEntity = new TestCoreEntity { EntityID = Guid.NewGuid(), Name = "Test Entity" };
            _ = mockDatabase.Setup(db => db.InsertEntityAsync(testEntity, null)).ReturnsAsync(1);

            // Act
            int result = await mockDatabase.Object.InsertEntityAsync(testEntity);

            // Assert
            _ = result.Should().Be(1);
            mockDatabase.Verify(db => db.InsertEntityAsync(testEntity, null), Times.Once);
        }

        [Fact]
        public async Task InsertOrReplaceEntityAsync_ShouldInsertOrReplaceEntitySuccessfully()
        {
            // Arrange
            var mockDatabase = new Mock<ICoreEntityDatabase>();
            var testEntity = new TestCoreEntity { EntityID = Guid.NewGuid(), Name = "Test Entity" };
            _ = mockDatabase.Setup(db => db.InsertOrReplaceEntityAsync(testEntity, null)).ReturnsAsync(1);

            // Act
            int result = await mockDatabase.Object.InsertOrReplaceEntityAsync(testEntity);

            // Assert
            _ = result.Should().Be(1);
            mockDatabase.Verify(db => db.InsertOrReplaceEntityAsync(testEntity, null), Times.Once);
        }

        [Fact]
        public async Task InsertOrUpdateEntityAsync_ShouldInsertOrUpdateEntitySuccessfully()
        {
            // Arrange
            var mockDatabase = new Mock<ICoreEntityDatabase>();
            var testEntity = new TestCoreEntity { EntityID = Guid.NewGuid(), Name = "Test Entity" };
            _ = mockDatabase.Setup(db => db.InsertOrUpdateEntityAsync(
                testEntity,
                It.IsAny<Func<TestCoreEntity, bool>>(),
                It.IsAny<Func<TestCoreEntity, TestCoreEntity>>(),
                null))
            .ReturnsAsync(testEntity);

            // Act
            TestCoreEntity? result = await mockDatabase.Object.InsertOrUpdateEntityAsync(
                testEntity,
                e => e.EntityID == testEntity.EntityID,
                e => e);

            // Assert
            _ = result.Should().Be(testEntity);
            mockDatabase.Verify(
                db => db.InsertOrUpdateEntityAsync(
                testEntity,
                It.IsAny<Func<TestCoreEntity, bool>>(),
                It.IsAny<Func<TestCoreEntity, TestCoreEntity>>(),
                null),
                Times.Once);
        }

        [Fact]
        public void ConvertEntity_ShouldConvertEntitySuccessfully()
        {
            // Arrange
            var mockDatabase = new Mock<ICoreEntityDatabase>();
            var testEntity = new TestCoreEntity { EntityID = Guid.NewGuid(), Name = "Test Entity" };
            _ = mockDatabase.Setup(db => db.ConvertEntity<TestCoreEntity>(testEntity, null)).Returns(testEntity);

            // Act
            TestCoreEntity? result = mockDatabase.Object.ConvertEntity<TestCoreEntity>(testEntity);

            // Assert
            _ = result.Should().Be(testEntity);
            mockDatabase.Verify(db => db.ConvertEntity<TestCoreEntity>(testEntity, null), Times.Once);
        }

        [Fact]
        public async Task GetEntityAsTypeAsync_ShouldRetrieveEntityByIdSuccessfully()
        {
            // Arrange
            var mockDatabase = new Mock<ICoreEntityDatabase>();
            var testEntity = new TestCoreEntity { EntityID = Guid.NewGuid(), Name = "Test Entity" };
            _ = mockDatabase.Setup(db => db.GetEntityAsTypeAsync<TestCoreEntity>(testEntity.EntityID, CoreEntityConstants.DefaultSnapshotID)).ReturnsAsync(testEntity);

            // Act
            TestCoreEntity? result = await mockDatabase.Object.GetEntityAsTypeAsync<TestCoreEntity>(testEntity.EntityID, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().Be(testEntity);
            mockDatabase.Verify(db => db.GetEntityAsTypeAsync<TestCoreEntity>(testEntity.EntityID, CoreEntityConstants.DefaultSnapshotID), Times.Once);
        }

        [Fact]
        public async Task GetEntityAsync_ShouldRetrieveEntityByLookupKeySuccessfully()
        {
            // Arrange
            var mockDatabase = new Mock<ICoreEntityDatabase>();
            var testEntity = new TestCoreEntity { EntityID = Guid.NewGuid(), Name = "Test Entity" };
            _ = mockDatabase.Setup(db => db.GetEntityAsync("TestKey", CoreEntityConstants.DefaultSnapshotID)).ReturnsAsync(testEntity);

            // Act
            CoreEntity? result = await mockDatabase.Object.GetEntityAsync("TestKey", CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().Be(testEntity);
            mockDatabase.Verify(db => db.GetEntityAsync("TestKey", CoreEntityConstants.DefaultSnapshotID), Times.Once);
        }

        [Fact]
        public async Task GetEntitiesAsync_ShouldRetrieveAllEntitiesSuccessfully()
        {
            // Arrange
            var mockDatabase = new Mock<ICoreEntityDatabase>();
            var testEntities = new List<CoreEntity>
        {
            new TestCoreEntity { EntityID = Guid.NewGuid(), Name = "Entity 1" },
            new TestCoreEntity { EntityID = Guid.NewGuid(), Name = "Entity 2" },
        };
            _ = mockDatabase.Setup(db => db.GetEntitiesAsync(CoreEntityConstants.DefaultSnapshotID)).ReturnsAsync(testEntities);

            // Act
            List<CoreEntity> result = await mockDatabase.Object.GetEntitiesAsync(CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().BeEquivalentTo(testEntities);
            mockDatabase.Verify(db => db.GetEntitiesAsync(CoreEntityConstants.DefaultSnapshotID), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteEntitySuccessfully()
        {
            // Arrange
            var mockDatabase = new Mock<ICoreEntityDatabase>();
            var testEntity = new TestCoreEntity { EntityID = Guid.NewGuid(), Name = "Test Entity" };
            _ = mockDatabase.Setup(db => db.DeleteEntityAsync(testEntity, CoreEntityConstants.DefaultSnapshotID)).ReturnsAsync(1);

            // Act
            int result = await mockDatabase.Object.DeleteEntityAsync(testEntity, CoreEntityConstants.DefaultSnapshotID);

            // Assert
            _ = result.Should().Be(1);
            mockDatabase.Verify(db => db.DeleteEntityAsync(testEntity, CoreEntityConstants.DefaultSnapshotID), Times.Once);
        }

        private record TestCoreEntity : CoreEntity
        {
            public string? Name { get; set; }
        }

#endif
    }
}
